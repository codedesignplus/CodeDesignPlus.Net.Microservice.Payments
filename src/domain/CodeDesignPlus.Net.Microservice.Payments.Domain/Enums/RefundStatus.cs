namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Lifecycle of a refund of a collected payment, normalized from what the provider reports.
/// </summary>
public enum RefundStatus
{
    /// <summary>
    /// The provider accepted it and is processing it.
    /// </summary>
    Pending = 1,
    /// <summary>
    /// The money went back to the payer.
    /// </summary>
    Approved = 2,
    /// <summary>
    /// The provider or the processor rejected it; no money left the merchant.
    /// </summary>
    Declined = 3,
    /// <summary>
    /// It was cancelled before the provider processed it.
    /// </summary>
    Cancelled = 4,
    /// <summary>
    /// It failed while being processed.
    /// </summary>
    Error = 5
}
