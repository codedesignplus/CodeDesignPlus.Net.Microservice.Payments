namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// The provider stopped accepting the keys of a collection account: the tenant no longer charges online until someone
/// replaces them. The administration must be told.
/// </summary>
[EventKey<MerchantAccountAggregate>(1, "MerchantAccountInvalidatedDomainEvent")]
public class MerchantAccountInvalidatedDomainEvent(
    Guid aggregateId,
    Guid tenant,
    string provider,
    string reason,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid Tenant { get; } = tenant;
    public string Provider { get; } = provider;

    /// <summary>
    /// What the provider rejected; never a secret.
    /// </summary>
    public string Reason { get; } = reason;

    public static MerchantAccountInvalidatedDomainEvent Create(Guid aggregateId, Guid tenant, string provider, string reason)
    {
        return new MerchantAccountInvalidatedDomainEvent(aggregateId, tenant, provider, reason);
    }
}
