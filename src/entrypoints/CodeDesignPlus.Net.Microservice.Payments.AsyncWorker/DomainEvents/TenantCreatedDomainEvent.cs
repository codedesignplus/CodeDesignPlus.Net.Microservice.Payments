namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.DomainEvents;

/// <summary>
/// ms-tenants created a tenant.
/// </summary>
/// <remarks>
/// Only what is used is declared: the country, which routes the tenant to a payment provider, and who created it. The
/// rest of the event is ignored when it is read.
/// </remarks>
[EventKey("TenantAggregate", 1, "TenantCreatedDomainEvent", "ms-tenants", "codedesignplus")]
public class TenantCreatedDomainEvent(
    Guid aggregateId,
    string name,
    TenantCreatedLocation location,
    Guid createdBy,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public string Name { get; } = name;
    public TenantCreatedLocation Location { get; } = location;
    public Guid CreatedBy { get; } = createdBy;
}

/// <summary>
/// Location of the created tenant.
/// </summary>
public class TenantCreatedLocation
{
    /// <summary>
    /// Country of the tenant.
    /// </summary>
    public TenantCreatedCountry Country { get; set; } = null!;
}

/// <summary>
/// Country of the created tenant.
/// </summary>
public class TenantCreatedCountry
{
    /// <summary>
    /// ISO 3166-1 alpha-2 code.
    /// </summary>
    public string Alpha2 { get; set; } = string.Empty;
}
