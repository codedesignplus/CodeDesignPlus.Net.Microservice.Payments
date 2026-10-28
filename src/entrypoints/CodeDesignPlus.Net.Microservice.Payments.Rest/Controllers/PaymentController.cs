using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ProcessProviderEvent;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.RefreshPaymentStatus;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPaymentSummaries;
using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

/// <summary>
/// Controller for handling payment operations.
/// </summary>
/// <param name="mediator">Mediator instance for sending commands.</param>
[Route("api/[controller]")]
[ApiController]
public class PaymentController(IMediator mediator) : ControllerBase
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
    /// The single webhook every merchant account posts its events to.
    /// </summary>
    /// <remarks>
    /// The body is read raw because the signature is computed over it. Whose secret verifies it is decided by the
    /// event's reference, so one URL serves every tenant's merchant and the platform's. An event that cannot be
    /// trusted is discarded and logged, and still answered 200 so the provider stops retrying it. When the Vault or
    /// the provider does not answer, it is answered 503 so the provider retries; the reconciliation job recovers what
    /// is left.
    /// </remarks>
    /// <param name="providerName">Code of the provider that posts the event (for example <c>wompi</c>).</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">The event was received; it was applied only if it could be trusted.</response>
    /// <response code="400">If the provider is not supported.</response>
    /// <response code="503">If the Vault or the provider did not answer; the provider should retry.</response>
    [HttpPost("notify/{providerName}")]
    [AllowAnonymous]
    public async Task<IActionResult> Notify(string providerName, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);

        var body = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            await mediator.Send(new ProcessProviderEventCommand(providerName, body), cancellationToken);
        }
        catch (CodeDesignPlusException exception) when (TransientFailures.IsTransient(exception))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        return Ok();
    }

    /// <summary>
    /// How a payment stands, asking the provider first when it is still in progress.
    /// </summary>
    /// <remarks>
    /// For the page the provider returns to: it never trusts what the provider appends to the URL and asks here.
    /// </remarks>
    /// <param name="id">Id of the payment.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">The status of the payment.</response>
    /// <response code="400">If the payment does not exist.</response>
    [HttpPost("{id:guid}/refresh")]
    public async Task<IActionResult> Refresh(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RefreshPaymentStatusCommand(id), cancellationToken);

        return Ok(result);
    }
}
