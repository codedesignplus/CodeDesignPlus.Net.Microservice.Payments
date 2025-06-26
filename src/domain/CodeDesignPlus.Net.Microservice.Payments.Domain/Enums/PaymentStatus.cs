namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
///  Represents the status of a payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// The payment has been initiated by the user, but its final result has not yet been confirmed.
    /// </summary>
    Initiated,
    /// <summary>
    /// The payment has been approved and the funds have been captured.
    /// </summary>
    Succeeded,
    /// <summary>
    ///  The payment was rejected by the provider, bank, or fraud rules.
    /// </summary>
    Failed
}
