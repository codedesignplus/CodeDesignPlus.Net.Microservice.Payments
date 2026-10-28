namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// A tenant got its collection account, still waiting for its provider keys.
/// </summary>
[EventKey<MerchantAccountAggregate>(1, "MerchantAccountCreatedDomainEvent")]
public class MerchantAccountCreatedDomainEvent(
    Guid aggregateId,
    Guid tenant,
    string country,
    string? provider,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid Tenant { get; } = tenant;
    public string Country { get; } = country;
    public string? Provider { get; } = provider;

    public static MerchantAccountCreatedDomainEvent Create(Guid aggregateId, Guid tenant, string country, string? provider)
    {
        return new MerchantAccountCreatedDomainEvent(aggregateId, tenant, country, provider);
    }
}
