using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

/// <summary>
/// Controller for managing payment methods.
/// </summary>
/// <param name="mediator">The mediator for handling requests.</param>
[Route("api/[controller]")]
[ApiController]
public class PaymentMethodController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves payment methods by provider.
    /// </summary>
    /// <param name="provider">The payment provider to filter the payment methods.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <param name="methods">Optional list of payment methods to filter by.</param>
    /// <response code="200">Returns a list of payment methods for the specified provider.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user is forbidden from accessing this resource.</response>
    [HttpGet("{provider}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TypePaymentMethod>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetPaymentMethodByProvider(PaymentProvider provider, [FromQuery] List<TypePaymentMethod> methods, CancellationToken cancellationToken)
    {
        var data = await mediator.Send(new GetPaymentMethodsByProviderQuery(provider, methods), cancellationToken);

        return Ok(data);
    }
}
