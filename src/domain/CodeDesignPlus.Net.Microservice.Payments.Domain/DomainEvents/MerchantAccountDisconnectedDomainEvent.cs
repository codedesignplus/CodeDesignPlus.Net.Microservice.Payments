namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// A tenant disconnected its collection account: its keys were deleted and it no longer charges online.
/// </summary>
[EventKey<MerchantAccountAggregate>(1, "MerchantAccountDisconnectedDomainEvent")]
public class MerchantAccountDisconnectedDomainEvent(
    Guid aggregateId,
    Guid tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid Tenant { get; } = tenant;

    public static MerchantAccountDisconnectedDomainEvent Create(Guid aggregateId, Guid tenant)
    {
        return new MerchantAccountDisconnectedDomainEvent(aggregateId, tenant);
    }
}
