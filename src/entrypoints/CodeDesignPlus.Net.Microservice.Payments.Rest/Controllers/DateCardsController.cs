using CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.Queries.GetDateCards;
using Microsoft.AspNetCore.Authorization;
using CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DateCardsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{year:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DateCardsDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> GetDateCards(int year)
    {
        var query = new GetDateCardsQuery(year);
        var dateCards = await mediator.Send(query);

        return Ok(dateCards);
    }
    
}
