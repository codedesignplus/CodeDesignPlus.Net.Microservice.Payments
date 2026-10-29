using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// The provider reported a new status for a refund of a collected payment.
/// </summary>
/// <remarks>
/// Generic on purpose: it says which payment, which refund, how it stands and how much, and the module that charged
/// decides what a refund means for it. Nobody consumes it yet; the refund flow is a later stage (pendings/660).
/// </remarks>
[EventKey<PaymentAggregate>(1, "PaymentRefundUpdatedDomainEvent")]
public class PaymentRefundUpdatedDomainEvent(
    Guid aggregateId,
    string module,
    Guid referenceId,
    Guid? tenant,
    string? provider,
    string? providerTransactionId,
    string refundId,
    RefundStatus status,
    Money? amount,
    string? code,
    string? message,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public string Module { get; } = module;
    public Guid ReferenceId { get; } = referenceId;
    public Guid? Tenant { get; } = tenant;
    public string? Provider { get; } = provider;
    public string? ProviderTransactionId { get; } = providerTransactionId;
    public string RefundId { get; } = refundId;
    public RefundStatus Status { get; } = status;

    /// <summary>
    /// What is refunded, in minor units; null when the provider did not say.
    /// </summary>
    public Money? Amount { get; } = amount;

    public string? Code { get; } = code;
    public string? Message { get; } = message;

    public static PaymentRefundUpdatedDomainEvent Create(Guid aggregateId, string module, Guid referenceId, Guid? tenant, string? provider, string? providerTransactionId, PaymentRefund refund)
    {
        return new PaymentRefundUpdatedDomainEvent(aggregateId, module, referenceId, tenant, provider, providerTransactionId, refund.RefundId, refund.Status, refund.Amount, refund.Code, refund.Message);
    }
}
