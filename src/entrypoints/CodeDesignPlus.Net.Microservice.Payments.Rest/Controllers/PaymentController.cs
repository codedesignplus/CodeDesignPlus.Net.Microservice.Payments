using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.TokenizeCard;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPaymentSummaries;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

/// <summary>
/// Controller for handling payment operations.
/// </summary>
/// <param name="mediator">Mediator instance for sending commands.</param>
/// <param name="adapterFactory">Factory for creating payment provider adapters.</param>
[Route("api/[controller]")]
[ApiController]
public class PaymentController(IMediator mediator, IPaymentProviderAdapterFactory adapterFactory) : ControllerBase
{
    /// <summary>
    /// Listado paginado de cobros, de solo consulta, para la administracion de la copropiedad.
    /// </summary>
    /// <remarks>
    /// Es de administracion: responde a "que cobros hay en curso y como acabaron", que hasta ahora no se
    /// podia ver en ninguna pantalla. Lo que de verdad se mira son los que llevan horas en <c>Iniciado</c>,
    /// asi que el filtro por estado y el orden por fecha llegan en el <c>Criteria</c>.
    /// <para>
    /// <b>No devuelve datos de tarjeta</b>, y no es una omision que se pueda relajar. Del medio de pago
    /// salen el tipo y los cuatro ultimos digitos, nada mas: el token de la pasarela permitiria cobrar, el
    /// codigo de seguridad no puede siquiera estar almacenado, y el nombre del titular y el documento del
    /// comprador son dato personal que esta pantalla no necesita para nada. Por eso responde
    /// <c>PaymentSummaryDto</c> y no <c>PaymentDto</c>, que es el contrato interno del microservicio.
    /// </para>
    /// <para>
    /// Tampoco salen <c>InitiateResponse</c> ni <c>FinalResponse</c>: son la respuesta cruda del proveedor
    /// y su contenido no lo decidimos nosotros.
    /// </para>
    /// </remarks>
    /// <param name="criteria">Filtros, orden y pagina.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelacion.</param>
    /// <response code="200">La pagina de cobros, sin datos de tarjeta ni del documento del comprador.</response>
    /// <response code="403">Si el usuario no tiene permiso para consultar los cobros.</response>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] C.Criteria criteria, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPaymentSummariesQuery(criteria), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Webhook endpoint to receive payment notifications from the payment provider.
    /// </summary>
    /// <param name="providerName">The name of the payment provider sending the notification.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">Returns the payment response details.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="403">If the user is forbidden from accessing this resource.</response>
    [HttpPost("notify/{providerName}")]
    [AllowAnonymous]
    public async Task<IActionResult> Notify(string providerName, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(providerName, true, out PaymentProvider provider))
            return BadRequest("Payment provider not valid.");

        var adapter = adapterFactory.GetAdapter(provider);

        var webhookResponse = await adapter.ProcessWebhookAsync(Request, cancellationToken);

        if (!webhookResponse.IsSignatureValid)
            return Forbid();

        var command = new UpdateStatusCommand(webhookResponse.PaymentId, webhookResponse.FinalStatus, webhookResponse.RawData);

        await mediator.Send(command, cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Tokenizes a credit card with the specified payment provider for future use.
    /// </summary>
    /// <param name="command">The tokenization request containing card details and provider.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">Returns the token and masked card data on success.</response>
    /// <response code="400">If the request data is invalid.</response>
    [HttpPost("tokenize")]
    public async Task<IActionResult> TokenizeCard([FromBody] TokenizeCardCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
