using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.CreateSavedCard;
using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.DeleteSavedCard;
using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.SetDefaultCard;
using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Queries.GetSavedCardById;
using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Queries.GetSavedCardsByUser;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

/// <summary>
/// Controller for managing saved cards.
/// </summary>
/// <param name="mediator">The mediator for handling requests.</param>
/// <param name="mapper">The mapper for transforming DTOs.</param>
[Route("api/[controller]")]
[ApiController]
public class SavedCardController(IMediator mediator, IMapper mapper) : ControllerBase
{
    /// <summary>
    /// Retrieves all saved cards for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">Returns the list of saved cards for the current user.</response>
    /// <response code="401">If the user is not authorized.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<SavedCardDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSavedCardsByUserQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a saved card by its ID.
    /// </summary>
    /// <param name="id">The ID of the saved card.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">Returns the saved card details.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="404">If the saved card is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SavedCardDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSavedCardByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Saves a new card for the current user.
    /// </summary>
    /// <param name="data">The card data to save.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="204">The card was saved successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public async Task<IActionResult> Create([FromBody] CreateSavedCardDto data, CancellationToken cancellationToken)
    {
        await mediator.Send(mapper.Map<CreateSavedCardCommand>(data), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Soft-deletes a saved card.
    /// </summary>
    /// <param name="id">The ID of the saved card to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="204">The card was deleted successfully.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="404">If the saved card is not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteSavedCardCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets a saved card as the default card for the current user.
    /// </summary>
    /// <param name="id">The ID of the saved card to set as default.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="204">The card was set as default successfully.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="404">If the saved card is not found.</response>
    [HttpPatch("{id:guid}/default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new SetDefaultCardCommand(id), cancellationToken);
        return NoContent();
    }
}
