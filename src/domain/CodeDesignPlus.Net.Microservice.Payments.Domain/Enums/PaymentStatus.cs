namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
///  Represents the status of a payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// The payment status is unknown or not set.
    /// </summary>
    Unknown,
    /// <summary>
    /// The payment has been initiated by the user, but its final result has not yet been confirmed.
    /// </summary>
    Initiated,
    /// <summary>
    /// The payment has been approved and the funds have been captured.
    /// </summary>
    Succeeded,
    /// <summary>
    /// The payment was rejected by the provider, bank, or fraud rules.
    /// </summary>
    Failed,
    /// <summary>
    /// The payment session has expired before completion.
    /// </summary>
    Expired,
    /// <summary>
    /// The payment is pending, meaning it has not yet been confirmed or rejected.
    /// </summary>
    Pending,
    /// <summary>
    /// The provider declined an attempt and the payer can still retry on the same hosted page until the payment's
    /// <c>RetryWindowEndsAt</c> (pendings/622). Not final: it ends as Succeeded if a retry is approved, or as Failed when the
    /// window closes without one.
    /// </summary>
    AwaitingRetry
}
