using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ConnectMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.DisconnectMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ReplaceMerchantAccountKeys;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ValidateMerchantAccount;
using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Queries.GetMerchantAccount;
using CodeDesignPlus.Net.Security.Abstractions;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

/// <summary>
/// The collection account of the tenant in session: its own merchant at the payment provider.
/// </summary>
/// <remarks>
/// The keys go in and never come back: the answers carry only the public key and the last four characters of each
/// secret.
/// </remarks>
/// <param name="mediator">Mediator instance for sending commands.</param>
/// <param name="user">The user in session, whose tenant owns the account.</param>
[Route("api/[controller]")]
[ApiController]
public class MerchantAccountController(IMediator mediator, IUserContext user) : ControllerBase
{
    /// <summary>
    /// The collection account of the tenant in session; a tenant without one is answered as waiting for its keys.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">The account, without secrets.</response>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMerchantAccountQuery(user.Tenant), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Connects the account with the keys of the tenant's own merchant, after validating them with the provider.
    /// </summary>
    /// <param name="command">The keys.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">The connected account, without secrets.</response>
    /// <response code="400">If the keys do not work for this deployment or the account is already connected.</response>
    [HttpPost("connect")]
    public async Task<IActionResult> Connect([FromBody] ConnectMerchantAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Replaces the keys of the connected account, after validating the new ones with the provider.
    /// </summary>
    /// <param name="command">The new keys.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">The account with its new keys, without secrets.</response>
    /// <response code="400">If the keys do not work for this deployment or the account is not connected.</response>
    [HttpPut("keys")]
    public async Task<IActionResult> ReplaceKeys([FromBody] ReplaceMerchantAccountKeysCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Checks the stored keys against the provider again: the account ends Active when they work and Invalid when the
    /// provider rejects them.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">The account after the check, without secrets.</response>
    /// <response code="400">If the account is not connected.</response>
    /// <response code="503">If the provider or the Vault did not answer; the account is left as it was.</response>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new ValidateMerchantAccountCommand(), cancellationToken);

            return Ok(result);
        }
        catch (CodeDesignPlusException exception) when (TransientFailures.IsTransient(exception))
        {
            return Problem(detail: exception.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Disconnects the account and deletes its keys; online payments stop until it is connected again.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="204">The account was disconnected.</response>
    /// <response code="400">If the account is not connected.</response>
    [HttpDelete]
    public async Task<IActionResult> Disconnect(CancellationToken cancellationToken)
    {
        await mediator.Send(new DisconnectMerchantAccountCommand(), cancellationToken);

        return NoContent();
    }
}
