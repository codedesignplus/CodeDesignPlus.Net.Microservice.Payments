using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPaymentSummaries;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Test.Controllers;

/// <summary>
/// Cubre el endpoint de consulta de pagos que la administracion necesita.
/// </summary>
/// <remarks>
/// La consulta <c>GetAllPayment</c> llevaba tiempo escrita en la capa de aplicacion sin publicar, y esa era
/// la unica razon por la que el token de la tarjeta y el codigo de seguridad no salian por REST. Al hacer
/// falta la pantalla, el camino corto era exponerla tal cual.
/// <para>
/// Lo que se fija aqui es que el endpoint despache <c>GetAllPaymentSummariesQuery</c>. Cambiarlo por la otra
/// consulta compilaria sin una sola queja —las dos devuelven una <c>Pagination</c>— y la fuga entraria sin
/// que nada se pusiera rojo.
/// </para>
/// </remarks>
public class PaymentQueryEndpointTest
{
    private readonly Mock<IMediator> mediator = new();

    private readonly Pagination<PaymentSummaryDto> pagina = Pagination<PaymentSummaryDto>.Create([], 0, 10, 0);

    [Fact]
    public async Task ElListadoDevuelveElContratoDeConsultaYNoElInterno()
    {
        mediator
            .Setup(x => x.Send(It.IsAny<GetAllPaymentSummariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagina);

        var response = await BuildController().GetAll(new C.Criteria(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);

        Assert.Same(pagina, ok.Value);
    }

    [Fact]
    public async Task ElCriteriaLlegaTalCualALaConsulta()
    {
        // El filtro por estado es lo que hace util la pantalla: lo que de verdad se mira son los cobros que
        // llevan horas en Iniciado. Si el controlador construyera un Criteria propio, ese filtro se perderia
        // por el camino sin dar error.
        GetAllPaymentSummariesQuery? enviada = null;

        mediator
            .Setup(x => x.Send(It.IsAny<GetAllPaymentSummariesQuery>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((query, _) => enviada = (GetAllPaymentSummariesQuery)query)
            .ReturnsAsync(pagina);

        var criteria = new C.Criteria { Filters = "Status=1", OrderBy = "CreatedAt", Limit = 25 };

        await BuildController().GetAll(criteria, CancellationToken.None);

        Assert.NotNull(enviada);
        Assert.Same(criteria, enviada!.Criteria);
    }

    private PaymentController BuildController() => new(mediator.Object, Mock.Of<IPaymentProviderAdapterFactory>());
}
