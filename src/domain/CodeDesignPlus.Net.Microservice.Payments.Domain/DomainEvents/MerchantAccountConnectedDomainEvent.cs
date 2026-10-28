namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// A tenant connected its provider keys: from now on it can charge online. Carries no key.
/// </summary>
[EventKey<MerchantAccountAggregate>(1, "MerchantAccountConnectedDomainEvent")]
public class MerchantAccountConnectedDomainEvent(
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

    public static MerchantAccountConnectedDomainEvent Create(Guid aggregateId, Guid tenant, string provider, MerchantAccountEnvironment environment, bool payoutsEnabled)
    {
        return new MerchantAccountConnectedDomainEvent(aggregateId, tenant, provider, environment, payoutsEnabled);
    }
}
