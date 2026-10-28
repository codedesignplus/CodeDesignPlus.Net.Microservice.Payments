using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// A module asked to pay a third party. Carries no bank account: only the beneficiary's id.
/// </summary>
[EventKey<PayoutAggregate>(1, "PayoutRequestedDomainEvent")]
public class PayoutRequestedDomainEvent(
    Guid aggregateId,
    Guid tenant,
    string module,
    Guid referenceId,
    PayoutPurpose purpose,
    Guid beneficiaryId,
    Money amount,
    Guid requestedBy,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid Tenant { get; } = tenant;
    public string Module { get; } = module;
    public Guid ReferenceId { get; } = referenceId;
    public PayoutPurpose Purpose { get; } = purpose;
    public Guid BeneficiaryId { get; } = beneficiaryId;
    public Money Amount { get; } = amount;
    public Guid RequestedBy { get; } = requestedBy;

    public static PayoutRequestedDomainEvent Create(Guid aggregateId, Guid tenant, string module, Guid referenceId, PayoutPurpose purpose, Guid beneficiaryId, Money amount, Guid requestedBy)
    {
        return new PayoutRequestedDomainEvent(aggregateId, tenant, module, referenceId, purpose, beneficiaryId, amount, requestedBy);
    }
}
