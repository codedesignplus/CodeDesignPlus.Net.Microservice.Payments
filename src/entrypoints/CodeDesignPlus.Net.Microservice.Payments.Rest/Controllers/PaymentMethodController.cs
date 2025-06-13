using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;

/// <summary>
/// Controller for managing payment methods.
/// </summary>
/// <param name="mediator">The mediator for handling requests.</param>
[Route("api/[controller]")]
[ApiController]
public class PaymentMethodController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Handles the request to get payment methods by provider.
    /// </summary>
    /// <param name="provider">The payment provider to filter the payment methods.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>204 No Content if successful.</returns>
    [HttpGet("{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPaymentMethodByProvider(Provider provider, CancellationToken cancellationToken)
    {
        var data = await mediator.Send(new GetPaymentMethodsByProviderQuery(provider), cancellationToken);

        return Ok(data);
    }
}
