using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.gRpc.Clients.Services.Notification;
using CodeDesignPlus.Net.ValueObjects.Financial;
// El namespace del micro tiene un segmento "Payment", y la busqueda del namespace envolvente gana a
// las directivas using: "PaymentMethod" a secas resuelve a un namespace, no al objeto de valor. Un
// alias con otro nombre lo esquiva.
using MetodoDePago = CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod;
using Tarjeta = CodeDesignPlus.Net.ValueObjects.Payment.CreditCard;
using CodeDesignPlus.Net.ValueObjects.User;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Commands.UpdateStatus;

/// <summary>
/// Cubre el aviso que sale cuando la pasarela rechaza un pago.
/// </summary>
/// <remarks>
/// Va por el canal efimero: el desenlace del pago ya es consultable, la pantalla de compra reconcilia
/// contra <c>GET Order/{id}</c>, y guardarlo solo llenaria la bandeja de ruido que nadie abre.
/// <para>
/// Lo que se fija aqui es el contrato con el frontend, porque <b>romperlo no se nota</b>:
/// <c>/purchase/processing</c> escucha exactamente <c>"OrderPaymentFailed"</c> y lee <c>{ message }</c>.
/// Si el nombre o la forma cambian, el push sale, el acuse dice success, y la pantalla se queda
/// esperando para siempre sin un solo error en ningun log.
/// </para>
/// </remarks>
public class UpdateStatusCommandHandlerTest
{
    private static readonly Guid PaymentId = Guid.Parse("aa000000-0000-4000-8000-000000000001");
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    private static readonly Guid Comprador = Guid.Parse("cc000000-0000-4000-8000-000000000003");

    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<IUserContext> user = new();
    private readonly Mock<IPubSub> pubsub = new();
    private readonly Mock<ILiveChannelGrpc> liveChannel = new();

    public UpdateStatusCommandHandlerTest()
    {
        user.SetupGet(x => x.Tenant).Returns(Tenant);
        user.SetupGet(x => x.IdUser).Returns(Comprador);

        repository
            .Setup(x => x.FindAsync<PaymentAggregate>(PaymentId, Tenant, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPayment());
    }

    [Fact]
    public async Task ARejectedPaymentPushesTheNameTheScreenIsListeningFor()
    {
        LiveUserPush? captured = await CaptureAsync(PaymentStatus.Failed);

        Assert.NotNull(captured);
        Assert.Equal("OrderPaymentFailed", captured!.EventName);
        Assert.Equal(Comprador.ToString(), captured.UserId);
        Assert.Equal(Tenant.ToString(), captured.Tenant);
    }

    [Fact]
    public async Task ARejectedPaymentKeepsTheShapeTheScreenReads()
    {
        LiveUserPush? captured = await CaptureAsync(PaymentStatus.Failed);

        // camelCase, y con la clave "message": es lo que declara OrderPaymentFailedPayload.
        Assert.Contains("\"message\"", captured!.JsonPayload);
        Assert.Contains(PaymentId.ToString(), captured.JsonPayload);
    }

    [Fact]
    public async Task ASuccessfulPaymentPushesNothing()
    {
        // El exito lo cuenta ms-licenses cuando termina de aprovisionar, no la pasarela.
        LiveUserPush? captured = await CaptureAsync(PaymentStatus.Succeeded);

        Assert.Null(captured);
    }

    [Fact]
    public async Task UnaSesionCaducadaSeAceptaYNoRevienta()
    {
        // PayU manda `state_pol=5` cuando la sesion expira, que es justo lo que hace una transaccion PSE que
        // el comprador no termina. El adaptador ya lo traducia a Expired y el agregado lo rechazaba: el aviso
        // reventaba, se iba a la cola de errores y el cobro se quedaba en vuelo para siempre. La pasarela nos
        // contaba el desenlace y lo tirabamos.
        var cobro = BuildPayment();

        repository
            .Setup(x => x.FindAsync<PaymentAggregate>(PaymentId, Tenant, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cobro);

        await CaptureAsync(PaymentStatus.Expired);

        Assert.Equal(PaymentStatus.Expired, cobro.Status);
    }

    [Fact]
    public async Task UnEstadoSinResolverSigueSiendoUnError()
    {
        // `Unknown` es lo que devuelve el adaptador para cualquier codigo que no sabe traducir. Aceptarlo
        // cerraria el cobro sin saber como acabo, que es peor que fallar ruidosamente.
        repository
            .Setup(x => x.FindAsync<PaymentAggregate>(PaymentId, Tenant, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPayment());

        await Assert.ThrowsAsync<CodeDesignPlusException>(() => CaptureAsync(PaymentStatus.Unknown));
    }

    private async Task<LiveUserPush?> CaptureAsync(PaymentStatus status)
    {
        LiveUserPush? captured = null;

        liveChannel
            .Setup(x => x.PushToUserAsync(It.IsAny<LiveUserPush>(), It.IsAny<CancellationToken>()))
            .Callback<LiveUserPush, CancellationToken>((r, _) => captured = r)
            .Returns(Task.CompletedTask);

        var handler = new UpdateStatusCommandHandler(repository.Object, user.Object, pubsub.Object, liveChannel.Object);

        await handler.Handle(
            new UpdateStatusCommand(PaymentId, status, new Dictionary<string, string?> { ["state"] = status.ToString() }),
            CancellationToken.None);

        return captured;
    }

    private static PaymentAggregate BuildPayment()
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");

        var comprador = Buyer.CreateWithoutShipping(
            Comprador, "Ana Restrepo", "+573001112233", "ana@example.com",
            TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        return PaymentAggregate.Create(
            PaymentId, "licenses", Guid.Parse("dd000000-0000-4000-8000-000000000004"),
            subTotal, tax, subTotal + tax, comprador, null,
            MetodoDePago.Create("CREDIT_CARD", null, Tarjeta.Create("tok_1", "4242", "2030/12", "Ana Restrepo", "123")),
            "Compra de licencia", PaymentProvider.Payu, Tenant, Comprador);
    }
}
