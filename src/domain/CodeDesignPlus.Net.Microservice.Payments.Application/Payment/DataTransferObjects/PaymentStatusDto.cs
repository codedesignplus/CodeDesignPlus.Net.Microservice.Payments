namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

/// <summary>
/// How a payment stands, for the page the provider returns to and for the modules that charged it.
/// </summary>
public class PaymentStatusDto
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
    /// What the provider captured, in minor units; only once the payment succeeded.
    /// </summary>
    public Net.ValueObjects.Financial.Money? CapturedAmount { get; set; }

    /// <summary>
    /// Code of the provider that processes the payment.
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// Builds the status of a payment.
    /// </summary>
    /// <param name="payment">The payment.</param>
    /// <returns>Its status.</returns>
    public static PaymentStatusDto From(PaymentAggregate payment) => new()
    {
        Id = payment.Id,
        Status = payment.Status,
        CapturedAmount = payment.CapturedAmount,
        Provider = payment.Provider
    };
}
