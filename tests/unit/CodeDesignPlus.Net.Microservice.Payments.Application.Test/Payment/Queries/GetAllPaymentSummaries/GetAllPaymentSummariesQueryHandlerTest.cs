using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPaymentSummaries;
using CodeDesignPlus.Net.Microservice.Payments.Application.Setup;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
// El namespace del micro tiene un segmento "Payment", y la busqueda del namespace envolvente gana a
// las directivas using: "PaymentMethod" a secas resuelve a un namespace, no al objeto de valor.
using MetodoDePago = CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod;
using Tarjeta = CodeDesignPlus.Net.ValueObjects.Payment.CreditCard;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Queries.GetAllPaymentSummaries;

/// <summary>
/// Cubre la consulta que alimenta la pantalla de cobros del administrador.
/// </summary>
/// <remarks>
/// Lo que se fija es el aislamiento entre copropiedades. Un pago lleva el nombre de quien lo hizo y el
/// importe que se le cobro: aunque el DTO no publique ningun secreto, servirle a una copropiedad los cobros
/// de otra sigue siendo una fuga, y el unico sitio donde se decide es esta llamada al repositorio.
/// </remarks>
public class GetAllPaymentSummariesQueryHandlerTest
{
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    private static readonly Guid PaymentId = Guid.Parse("aa000000-0000-4000-8000-000000000001");
    private static readonly Guid Comprador = Guid.Parse("cc000000-0000-4000-8000-000000000003");
    private static readonly Guid Referencia = Guid.Parse("dd000000-0000-4000-8000-000000000004");

    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<IUserContext> user = new();

    public GetAllPaymentSummariesQueryHandlerTest()
    {
        MapsterConfigPayment.Configure();

        user.SetupGet(x => x.Tenant).Returns(Tenant);
    }

    [Fact]
    public async Task LaBusquedaSeAcotaALaCopropiedadDeQuienConsulta()
    {
        Guid? consultado = null;

        repository
            .Setup(x => x.MatchingAsync<PaymentAggregate>(It.IsAny<C.Criteria>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback<C.Criteria, Guid, CancellationToken>((_, tenant, _) => consultado = tenant)
            .ReturnsAsync(Pagination<PaymentAggregate>.Create([], 0, 10, 0));

        await Handle();

        Assert.Equal(Tenant, consultado);
    }

    [Fact]
    public async Task LoQueDevuelveYaVieneSinDatosDeTarjeta()
    {
        // El handler devuelve el contrato de consulta, no el agregado. Si alguien cambiara el tipo de
        // retorno al DTO interno, el token y el codigo de seguridad saldrian por el endpoint sin que nada
        // mas en el sistema tuviera que cambiar.
        repository
            .Setup(x => x.MatchingAsync<PaymentAggregate>(It.IsAny<C.Criteria>(), Tenant, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Pagination<PaymentAggregate>.Create([Construir()], 1, 10, 0));

        var pagina = await Handle();

        var resumen = Assert.Single(pagina.Data);

        Assert.Equal(PaymentId, resumen.Id);
        Assert.Equal("4242", resumen.Last4Digits);
        Assert.Equal("Ana Restrepo", resumen.BuyerName);
        Assert.Equal(1L, pagina.TotalCount);
    }

    private Task<Pagination<PaymentSummaryDto>> Handle()
    {
        var handler = new GetAllPaymentSummariesQueryHandler(repository.Object, new Mapper(TypeAdapterConfig.GlobalSettings), user.Object);

        return handler.Handle(new GetAllPaymentSummariesQuery(new C.Criteria()), CancellationToken.None);
    }

    private static PaymentAggregate Construir()
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");

        var comprador = Buyer.CreateWithoutShipping(
            Comprador, "Ana Restrepo", "+573001112233", "ana@example.com",
            TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        return PaymentAggregate.Create(
            PaymentId, "licenses", Referencia, subTotal, tax, subTotal + tax, comprador, null,
            MetodoDePago.Create("VISA", null, Tarjeta.Create("tok-sonda", "4242", "2030/12", "SONDA TITULAR", "sonda-cvv")),
            "Compra de licencia", PaymentProvider.Payu, Tenant, Comprador);
    }
}
