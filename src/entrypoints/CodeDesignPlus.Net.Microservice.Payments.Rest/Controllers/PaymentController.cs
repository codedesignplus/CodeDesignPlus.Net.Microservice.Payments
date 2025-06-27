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
    /// <returns>204 No Content if successful.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(PaymentResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckAndUpdate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateStatusCommand(id), cancellationToken);

        return Ok(result);
    }
}
