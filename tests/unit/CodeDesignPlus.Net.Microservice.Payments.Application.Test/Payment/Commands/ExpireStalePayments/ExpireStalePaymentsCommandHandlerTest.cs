using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ExpireStalePayments;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Microsoft.Extensions.Logging;
// El namespace del micro tiene un segmento "Payment", y la busqueda del namespace envolvente gana a
// las directivas using: "PaymentMethod" a secas resuelve a un namespace, no al objeto de valor.
using MetodoDePago = CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod;
using Tarjeta = CodeDesignPlus.Net.ValueObjects.Payment.CreditCard;
using CodeDesignPlus.Net.ValueObjects.User;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Commands.ExpireStalePayments;

/// <summary>
/// Cubre el barrido que cierra los cobros que se quedaron en vuelo.
/// </summary>
/// <remarks>
/// Fase 5 del plan de pagos en vuelo. Un comprador abre la pasarela, se va, y el cobro se queda en
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
    private static readonly Guid Comprador = Guid.Parse("cc000000-0000-4000-8000-000000000003");
    private static readonly Guid Documento = Guid.Parse("6cc38dea-d7c4-5d5d-a5f7-8a6394e5b166");

    private static readonly Duration UnDia = Duration.FromDays(1);

    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<IPubSub> pubsub = new();

    private readonly List<IDomainEvent> publicados = [];

    public ExpireStalePaymentsCommandHandlerTest()
    {
        repository
            .Setup(x => x.UpdateAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        pubsub
            .Setup(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<IDomainEvent>, CancellationToken>((events, _) => publicados.AddRange(events))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task UnCobroColgadoSeCierraComoExpirado()
    {
        var colgado = Cobro();

        Encuentra(colgado);

        await Handle();

        Assert.Equal(PaymentStatus.Expired, colgado.Status);
    }

    [Fact]
    public async Task ElCierreAnunciaElEventoQueFacturacionEscucha()
    {
        // **Es el punto entero de la fase.** Facturacion trata Expired igual que Failed y retira la
        // cotizacion de ese pago, asi que un solo cierre resuelve las dos limpiezas. Sin este evento, el
        // cobro queda cerrado aqui y la cotizacion viva alli, y nada avisa.
        var colgado = Cobro();

        Encuentra(colgado);

        await Handle();

        var evento = Assert.Single(publicados.OfType<PaymentResponseAssociatedDomainEvent>());

        Assert.Equal(PaymentStatus.Expired, evento.Status);
        Assert.Equal(Documento, evento.ReferenceId);
        Assert.Equal(Tenant, evento.Tenant);
    }

    [Fact]
    public async Task ElCierreDiceQueLoDecidioLaPlataforma()
    {
        // Quien lea el registro manana tiene que poder distinguir un rechazo del banco de un cierre nuestro
        // por silencio. Sin esto, los dos se ven igual: un cobro que no prospero.
        var colgado = Cobro();

        Encuentra(colgado);

        await Handle();

        Assert.Equal("platform", colgado.FinalResponse["expiredBy"]);
    }

    [Fact]
    public async Task SinCobrosColgadosNoSeAnunciaNada()
    {
        Encuentra();

        await Handle();

        Assert.Empty(publicados);
        repository.Verify(x => x.UpdateAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnCobroQueFallaNoImpideCerrarLosDemas()
    {
        // Y al terminar se lanza, porque continuar y decir que todo fue bien son cosas distintas: sin esto la
        // corrida sale en verde en el panel con cobros sin cerrar.
        var malo = Cobro();
        var bueno = Cobro();

        Encuentra(malo, bueno);

        repository
            .Setup(x => x.UpdateAsync(malo, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("mongo caido"));

        await Assert.ThrowsAsync<InvalidOperationException>(Handle);

        Assert.Equal(PaymentStatus.Expired, bueno.Status);
        Assert.Single(publicados.OfType<PaymentResponseAssociatedDomainEvent>());
    }

    [Fact]
    public async Task ElCorteSeCalculaDesdeElPlazoPedido()
    {
        Instant? corte = null;

        repository
            .Setup(x => x.GetInFlightOlderThanAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<Instant, int, CancellationToken>((c, _, __) => corte = c)
            .ReturnsAsync([]);

        var ahora = SystemClock.Instance.GetCurrentInstant();

        await Handle();

        Assert.NotNull(corte);

        // Un dia atras, con holgura para lo que tarde la prueba.
        var esperado = ahora.Minus(UnDia);

        Assert.True((corte!.Value - esperado).TotalSeconds is > -5 and < 5, $"corte={corte} esperado={esperado}");
    }

    private void Encuentra(params PaymentAggregate[] cobros) =>
        repository
            .Setup(x => x.GetInFlightOlderThanAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. cobros]);

    private Task Handle()
    {
        var handler = new ExpireStalePaymentsCommandHandler(
            repository.Object, pubsub.Object, Mock.Of<ILogger<ExpireStalePaymentsCommandHandler>>());

        return handler.Handle(new ExpireStalePaymentsCommand(UnDia, 500), CancellationToken.None);
    }

    /// <summary>Un cobro de facturacion, que es el que le importa a esta fase.</summary>
    /// <remarks>
    /// Nace en <c>Initiated</c>, que es el unico estado en vuelo que hoy se puede alcanzar: nada escribe
    /// <c>Pending</c> todavia. El barrido lo incluye igual porque el enum lo tiene y PayU lo reporta; el dia
    /// que llegue, ya estara cubierto.
    /// </remarks>
    private static PaymentAggregate Cobro()
    {
        var subTotal = Money.FromLong(80_216_669L, "COP");
        var tax = Money.FromLong(0L, "COP");

        var comprador = Buyer.CreateWithoutShipping(
            Comprador, "Jorge Enrique Montoya", "+573001112233", "jorge@example.com",
            TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        var cobro = PaymentAggregate.Create(
            Guid.NewGuid(), "Invoicing", Documento,
            subTotal, tax, subTotal + tax, comprador, null,
            MetodoDePago.Create("PSE", null, Tarjeta.Create("tok_1", "4242", "2030/12", "Jorge Enrique Montoya", "123")),
            "Pago documento CC-000003", PaymentProvider.Payu, Tenant, Comprador);

        cobro.GetAndClearEvents();

        return cobro;
    }
}
