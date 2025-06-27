using CodeDesignPlus.Net.Microservice.Payments.Application.Banks.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;


/// <summary>
/// Controller for handling bank-related operations.
/// </summary>
/// <param name="mediator">Mediator instance for sending commands.</param>
[Route("api/[controller]")]
[ApiController]
public class BankController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a list of all banks.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>204 No Content if successful.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BanksDto>))]
    public async Task<IActionResult> GetAllBanks(CancellationToken cancellationToken)
    {
        var data = await mediator.Send(new GetAllBanksQuery(), cancellationToken);

        return Ok(data);
    }
}
