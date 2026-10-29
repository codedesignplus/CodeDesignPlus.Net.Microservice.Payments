using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// The tenant set the rates its provider charges it: the commission of the next payments is computed with them.
/// </summary>
[EventKey<MerchantAccountAggregate>(1, "MerchantAccountFeeScheduleChangedDomainEvent")]
public class MerchantAccountFeeScheduleChangedDomainEvent(
    Guid aggregateId,
    Guid tenant,
    ProviderFeeSchedule feeSchedule,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid Tenant { get; } = tenant;
    public ProviderFeeSchedule FeeSchedule { get; } = feeSchedule;

    public static MerchantAccountFeeScheduleChangedDomainEvent Create(Guid aggregateId, Guid tenant, ProviderFeeSchedule feeSchedule)
    {
        return new MerchantAccountFeeScheduleChangedDomainEvent(aggregateId, tenant, feeSchedule);
    }
}
