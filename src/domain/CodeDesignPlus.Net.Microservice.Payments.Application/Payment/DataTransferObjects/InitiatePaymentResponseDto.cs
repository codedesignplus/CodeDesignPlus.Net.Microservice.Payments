using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

/// <summary>
/// What the caller does after starting a payment: send the payer to the provider's hosted page.
/// </summary>
public class InitiatePaymentResponseDto
{
    /// <summary>
    /// Whether the payment was started.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Id of the payment in this service.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// What to do next; always <see cref="NextActionType.Redirect"/>.
    /// </summary>
    public NextActionType NextAction { get; set; }

    /// <summary>
    /// The provider's hosted page.
    /// </summary>
    public string? RedirectUrl { get; set; }
}
