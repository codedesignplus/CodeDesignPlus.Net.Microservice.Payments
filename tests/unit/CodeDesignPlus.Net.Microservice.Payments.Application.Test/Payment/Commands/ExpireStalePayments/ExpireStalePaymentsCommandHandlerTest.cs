using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ExpireStalePayments;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Microsoft.Extensions.Logging;
// El namespace del micro tiene un segmento "Payment", y la busqueda del namespace envolvente gana a
// las directivas using: "PaymentMethod" a secas resuelve a un namespace, no al objeto de valor.
using GatewayMethod = CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod;
using Card = CodeDesignPlus.Net.ValueObjects.Payment.CreditCard;
using CodeDesignPlus.Net.ValueObjects.User;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Commands.ExpireStalePayments;

/// <summary>
/// Cubre el barrido que cierra los cobros que se quedaron en curso.
/// </summary>
/// <remarks>
/// Fase 5 del plan de pagos en curso. Un comprador abre la pasarela, se va, y el cobro se queda en
/// <c>Initiated</c> para siempre: el proveedor solo avisa de lo que resuelve, no de lo que se abandona.
/// <para>
/// Lo que de verdad se protege aqui no esta en este servicio: al cerrarlos, el agregado levanta
/// <see cref="PaymentResponseAssociatedDomainEvent"/>, que facturacion ya consume para retirar la cotizacion
/// que congelaba el importe a cobrar. <b>Si el evento dejara de salir, la cotizacion se quedaria viva</b> y
/// nada fallaria en ninguno de los dos servicios: el saldo del propietario se queda retenido en silencio.
/// </para>
/// </remarks>
public class ExpireStalePaymentsCommandHandlerTest
{
    private static readonly Guid Tenant = Guid.Parse("20d2459d-674e-476e-adb4-dcb0f7a224fa");
    private static readonly Guid Buyer = Guid.Parse("cc000000-0000-4000-8000-000000000003");
    private static readonly Guid Document = Guid.Parse("6cc38dea-d7c4-5d5d-a5f7-8a6394e5b166");

    private static readonly Duration OneDay = Duration.FromDays(1);

    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<IPubSub> pubsub = new();

    private readonly List<IDomainEvent> published = [];

    public ExpireStalePaymentsCommandHandlerTest()
    {
        repository
            .Setup(x => x.UpdateAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        pubsub
            .Setup(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<IDomainEvent>, CancellationToken>((events, _) => published.AddRange(events))
            .Returns(Task.CompletedTask);
    }

    /// <summary>Un cobro colgado se cierra como expirado.</summary>
    [Fact]
    public async Task AStrandedPaymentIsClosedAsExpired()
    {
        var stranded = Payment();

        Found(stranded);

        await Handle();

        Assert.Equal(PaymentStatus.Expired, stranded.Status);
    }

    /// <summary>El cierre anuncia el evento que facturacion escucha.</summary>
    [Fact]
    public async Task ClosingAnnouncesTheEventInvoicingListensFor()
    {
        // **Es el punto entero de la fase.** Facturacion trata Expired igual que Failed y retira la
        // cotizacion de ese pago, asi que un solo cierre resuelve las dos limpiezas. Sin este evento, el
        // cobro queda cerrado aqui y la cotizacion viva alli, y nada avisa.
        var stranded = Payment();

        Found(stranded);

        await Handle();

        var published = Assert.Single(this.published.OfType<PaymentResponseAssociatedDomainEvent>());

        Assert.Equal(PaymentStatus.Expired, published.Status);
        Assert.Equal(Document, published.ReferenceId);
        Assert.Equal(Tenant, published.Tenant);
    }

    /// <summary>El cierre deja dicho que lo decidio la plataforma.</summary>
    [Fact]
    public async Task ClosingRecordsThatThePlatformDecidedIt()
    {
        // Quien lea el registro manana tiene que poder distinguir un rechazo del banco de un cierre nuestro
        // por silencio. Sin esto, los dos se ven igual: un cobro que no prospero.
        var stranded = Payment();

        Found(stranded);

        await Handle();

        Assert.Equal("platform", stranded.FinalResponse["expiredBy"]);
    }

    /// <summary>Sin cobros colgados no se anuncia nada.</summary>
    [Fact]
    public async Task WithNoStrandedPaymentsNothingIsAnnounced()
    {
        Found();

        await Handle();

        Assert.Empty(published);
        repository.Verify(x => x.UpdateAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>Un cobro que falla no impide cerrar los demas.</summary>
    [Fact]
    public async Task AFailingPaymentDoesNotStopTheOthers()
    {
        // Y al terminar se lanza, porque continuar y decir que todo fue bien son cosas distintas: sin esto la
        // corrida sale en verde en el panel con cobros sin cerrar.
        var bad = Payment();
        var good = Payment();

        Found(bad, good);

        repository
            .Setup(x => x.UpdateAsync(bad, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("mongo caido"));

        await Assert.ThrowsAsync<InvalidOperationException>(Handle);

        Assert.Equal(PaymentStatus.Expired, good.Status);
        Assert.Single(published.OfType<PaymentResponseAssociatedDomainEvent>());
    }

    /// <summary>El corte se calcula desde el plazo pedido.</summary>
    [Fact]
    public async Task TheCutoffIsDerivedFromTheRequestedWindow()
    {
        Instant? cutoff = null;

        repository
            .Setup(x => x.GetInProgressOlderThanAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Instant, int, CancellationToken>((c, _, __) => cutoff = c)
            .ReturnsAsync([]);

        var now = SystemClock.Instance.GetCurrentInstant();

        await Handle();

        Assert.NotNull(cutoff);

        var expected = now.Minus(OneDay);

        Assert.True((cutoff!.Value - expected).TotalSeconds is > -5 and < 5, $"cutoff={cutoff} expected={expected}");
    }

    private void Found(params PaymentAggregate[] payments) =>
        repository
            .Setup(x => x.GetInProgressOlderThanAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. payments]);

    private Task Handle()
    {
        var handler = new ExpireStalePaymentsCommandHandler(
            repository.Object, pubsub.Object, Mock.Of<ILogger<ExpireStalePaymentsCommandHandler>>());

        return handler.Handle(new ExpireStalePaymentsCommand(OneDay, 500), CancellationToken.None);
    }

    /// <summary>Un cobro de facturacion, que es el que le importa a esta fase.</summary>
    /// <remarks>
    /// Nace en <c>Initiated</c>, que es el unico estado en curso que hoy se puede alcanzar: nada escribe
    /// <c>Pending</c> todavia. El barrido lo incluye igual porque el enum lo tiene y PayU lo reporta; el dia
    /// que llegue, ya estara cubierto.
    /// </remarks>
    private static PaymentAggregate Payment()
    {
        var subTotal = Money.FromLong(80_216_669L, "COP");
        var tax = Money.FromLong(0L, "COP");

        var buyer = Net.ValueObjects.User.Buyer.CreateWithoutShipping(
            Buyer, "Jorge Enrique Montoya", "+573001112233", "jorge@example.com",
            TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        var payment = PaymentAggregate.Create(
            Guid.NewGuid(), "Invoicing", Document,
            subTotal, tax, subTotal + tax, buyer, null,
            GatewayMethod.Create("PSE", null, Card.Create("tok_1", "4242", "2030/12", "Jorge Enrique Montoya", "123")),
            "Pago documento CC-000003", PaymentProvider.Payu, Tenant, Buyer);

        payment.GetAndClearEvents();

        return payment;
    }
}
