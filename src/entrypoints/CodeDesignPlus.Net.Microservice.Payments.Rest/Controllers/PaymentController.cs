using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;
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
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <response code="200">Returns the payment response details.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="403">If the user is forbidden from accessing this resource.</response>
    [HttpPost("[action]")]
    [AllowAnonymous]
    public async Task<IActionResult> Notify(CancellationToken cancellationToken)
    {
        if (Request.Form.ContainsKey("merchant_id"))
        {
            var adapter = adapterFactory.GetAdapter(PaymentProvider.Payu);

            var merchantId = Request.Form["merchant_id"];
            var currency = Request.Form["currency"];
            var state = Request.Form["state_pol"];
            var value = Request.Form["value"];
            var signatureReceived = Request.Form["sign"];
            var referenceSale = Request.Form["reference_sale"];

            InfrastructureGuard.IsNullOrEmpty(merchantId, Errors.MerchantIdIsRequired);
            InfrastructureGuard.IsNullOrEmpty(currency, Errors.CurrencyIsRequired);
            InfrastructureGuard.IsNullOrEmpty(state, Errors.StateIsRequired);
            InfrastructureGuard.IsNullOrEmpty(value, Errors.ValueIsRequired);
            InfrastructureGuard.IsNullOrEmpty(signatureReceived, Errors.SignatureIsRequired);
            InfrastructureGuard.IsNullOrEmpty(referenceSale, Errors.ReferenceCodeIsRequired);
            InfrastructureGuard.IsFalse(Guid.TryParse(referenceSale, out var reference), Errors.ReferenceCodeIsInvalid);

            var signatureIsValid = await adapter.CheckSignature(merchantId!, reference!, value!, currency!, state!, signatureReceived!);

            var command = new UpdateStatusCommand(
                reference,
                state == "4" ? PaymentStatus.Succeeded : PaymentStatus.Failed,
                Request.Form.Keys.ToDictionary(k => k, k => Request.Form[k].ToString())
            );

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        return BadRequest("Invalid provider notification.");
    }
}
