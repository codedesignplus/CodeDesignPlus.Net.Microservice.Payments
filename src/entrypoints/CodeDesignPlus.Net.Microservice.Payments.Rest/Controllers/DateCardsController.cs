using CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.Queries.GetDateCards;
using Microsoft.AspNetCore.Authorization;
using CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DateCardsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves a list of date cards.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>200 OK with a list of date cards if successful.</returns>
    /// <response code="200">Returns a list of date cards.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user is forbidden from accessing this resource.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DateCardsDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    [AllowAnonymous]
    public async Task<IActionResult> GetDateCards(CancellationToken cancellationToken)
    {
        var query = new GetDateCardsQuery();
        var dateCards = await mediator.Send(query, cancellationToken);

        return Ok(dateCards);
    }
    
}
