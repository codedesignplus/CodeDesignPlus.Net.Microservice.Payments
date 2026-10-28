namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// A tenant replaced the provider keys of its collection account. Carries no key.
/// </summary>
[EventKey<MerchantAccountAggregate>(1, "MerchantAccountKeysReplacedDomainEvent")]
public class MerchantAccountKeysReplacedDomainEvent(
    Guid aggregateId,
    Guid tenant,
    string provider,
    MerchantAccountEnvironment environment,
    bool payoutsEnabled,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid Tenant { get; } = tenant;
    public string Provider { get; } = provider;
    public MerchantAccountEnvironment Environment { get; } = environment;
    public bool PayoutsEnabled { get; } = payoutsEnabled;

    public static MerchantAccountKeysReplacedDomainEvent Create(Guid aggregateId, Guid tenant, string provider, MerchantAccountEnvironment environment, bool payoutsEnabled)
    {
        return new MerchantAccountKeysReplacedDomainEvent(aggregateId, tenant, provider, environment, payoutsEnabled);
    }
}
