namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// The stored keys of an invalidated collection account work again: the tenant charges online again.
/// </summary>
[EventKey<MerchantAccountAggregate>(1, "MerchantAccountRevalidatedDomainEvent")]
public class MerchantAccountRevalidatedDomainEvent(
    Guid aggregateId,
    Guid tenant,
    string provider,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid Tenant { get; } = tenant;
    public string Provider { get; } = provider;

    public static MerchantAccountRevalidatedDomainEvent Create(Guid aggregateId, Guid tenant, string provider)
    {
        return new MerchantAccountRevalidatedDomainEvent(aggregateId, tenant, provider);
    }
}
