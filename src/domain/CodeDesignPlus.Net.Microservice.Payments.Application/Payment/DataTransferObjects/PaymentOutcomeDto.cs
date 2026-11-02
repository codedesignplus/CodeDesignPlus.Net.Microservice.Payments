namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

/// <summary>
/// How a payment ended, for the screen that tells the payer why it did not go through (pendings/622).
/// </summary>
/// <remarks>
/// It carries no card data and none of the provider's raw response: only the normalized status, the reason and what the
/// bank or the provider said.
/// </remarks>
public class PaymentOutcomeDto
{
    /// <summary>
    /// Id of the payment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Current status.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Why it did not succeed; <see cref="PaymentFailureReason.None"/> while in progress, when it succeeded or when it
    /// closed before the reason was recorded.
    /// </summary>
    public PaymentFailureReason FailureReason { get; set; }

    /// <summary>
    /// What the bank or the provider said, as they said it; null when they said nothing.
    /// </summary>
    public string? ProviderMessage { get; set; }

    /// <summary>
    /// Builds the outcome of a payment.
    /// </summary>
    /// <param name="payment">The payment.</param>
    /// <returns>Its outcome.</returns>
    public static PaymentOutcomeDto From(PaymentAggregate payment) => new()
    {
        Id = payment.Id,
        Status = payment.Status,
        FailureReason = payment.FailureReason,
        ProviderMessage = payment.ProviderMessage
    };
}
