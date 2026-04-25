using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.TokenizeCard;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

/// <summary>
/// Controller for handling payment operations.
/// </summary>
/// <param name="mediator">Mediator instance for sending commands.</param>
/// <param name="adapterFactory">Factory for creating payment provider adapters.</param>
[Route("api/[controller]")]
[ApiController]
public class PaymentController(IMediator mediator, IPaymentProviderAdapterFactory adapterFactory) : ControllerBase
{
    /// <summary>
    /// Webhook endpoint to receive payment notifications from the payment provider.
    /// </summary>
    /// <param name="providerName">The name of the payment provider sending the notification.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">Returns the payment response details.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="403">If the user is forbidden from accessing this resource.</response>
    [HttpPost("notify/{providerName}")]
    [AllowAnonymous]
    public async Task<IActionResult> Notify(string providerName, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(providerName, true, out PaymentProvider provider))
            return BadRequest("Payment provider not valid.");

        var adapter = adapterFactory.GetAdapter(provider);

        var webhookResponse = await adapter.ProcessWebhookAsync(Request, cancellationToken);

        if (!webhookResponse.IsSignatureValid)
            return Forbid();

        var command = new UpdateStatusCommand(webhookResponse.PaymentId, webhookResponse.FinalStatus, webhookResponse.RawData);

        await mediator.Send(command, cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Tokenizes a credit card with the specified payment provider for future use.
    /// </summary>
    /// <param name="command">The tokenization request containing card details and provider.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">Returns the token and masked card data on success.</response>
    /// <response code="400">If the request data is invalid.</response>
    [HttpPost("tokenize")]
    public async Task<IActionResult> TokenizeCard([FromBody] TokenizeCardCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
