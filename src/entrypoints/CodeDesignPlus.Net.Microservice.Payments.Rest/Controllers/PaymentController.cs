namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

/// <summary>
/// Controller for handling payment operations.
/// </summary>
/// <param name="mediator">Mediator instance for sending commands.</param>
/// <param name="mapper">Mapper instance for mapping DTOs to commands.</param>
[Route("api/[controller]")]
[ApiController]
public class PaymentController(IMediator mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Processes a payment request.
    /// </summary>
    /// <param name="data">The payment data to process.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>204 No Content if successful.</returns>
    [HttpPost]
    public async Task<IActionResult> Pay([FromBody] PayDto data, CancellationToken cancellationToken)
    {
        await mediator.Send(mapper.Map<PayCommand>(data), cancellationToken);

        return NoContent();
    }
}
