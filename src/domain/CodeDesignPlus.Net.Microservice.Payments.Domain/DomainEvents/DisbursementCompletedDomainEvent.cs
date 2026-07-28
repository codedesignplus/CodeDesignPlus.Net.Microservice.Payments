namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<DisbursementAggregate>(2, "DisbursementCompletedDomainEvent")]
public class DisbursementCompletedDomainEvent(
    Guid aggregateId,
    Guid beneficiaryId,
    long disbursedAmount,
    string currency,
    string providerReference,
    Guid tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid BeneficiaryId { get; } = beneficiaryId;
    public long DisbursedAmount { get; } = disbursedAmount;

    /// <summary>El codigo de moneda ISO 4217 de <see cref="DisbursedAmount"/>.</summary>
    public string Currency { get; } = currency;

    public string ProviderReference { get; } = providerReference;
    public Guid Tenant { get; } = tenant;

    public static DisbursementCompletedDomainEvent Create(
        Guid aggregateId,
        Guid beneficiaryId,
        long disbursedAmount,
        string currency,
        string providerReference,
        Guid tenant,
        Guid? eventId = null,
        Instant? occurredAt = null,
        Dictionary<string, object>? metadata = null)
    {
        return new DisbursementCompletedDomainEvent(
            aggregateId,
            beneficiaryId,
            disbursedAmount,
            currency,
            providerReference,
            tenant,
            eventId ?? Guid.NewGuid(),
            occurredAt ?? SystemClock.Instance.GetCurrentInstant(),
            metadata
        );
    }
}
