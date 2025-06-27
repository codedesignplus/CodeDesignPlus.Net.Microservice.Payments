using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

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
    /// Processes a payment request.
    /// </summary>
    /// <param name="id">Identifier of the payment to process.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">Returns the payment response details.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user is forbidden from accessing this resource.</response> 
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> CheckAndUpdate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateStatusCommand(id), cancellationToken);

        return Ok(result);
    }
}
