using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

/// <summary>
/// A refund of a collected payment, as the provider last reported it.
/// </summary>
/// <param name="RefundId">Id of the refund at the provider.</param>
/// <param name="Status">Its normalized status.</param>
/// <param name="Amount">What is refunded, in minor units; null when the provider did not say.</param>
/// <param name="Code">Code of the provider's reason, when the refund did not go through.</param>
/// <param name="Message">The provider's reason in its own words, when the refund did not go through.</param>
/// <param name="UpdatedAt">When this service learned the status.</param>
public sealed record PaymentRefund(string RefundId, RefundStatus Status, Money? Amount, string? Code, string? Message, Instant UpdatedAt)
{
    /// <summary>
    /// Whether the refund reached a status it can no longer leave.
    /// </summary>
    public bool IsFinal => Status is not RefundStatus.Pending;
}
