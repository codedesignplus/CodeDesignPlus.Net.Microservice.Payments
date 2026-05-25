namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<DisbursementAggregate>(1, "DisbursementCreatedDomainEvent")]
public class DisbursementCreatedDomainEvent(
    Guid aggregateId,
    Guid paymentId,
    Guid beneficiaryId,
    long disbursedAmount,
    string currency,
    Guid tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid PaymentId { get; } = paymentId;
    public Guid BeneficiaryId { get; } = beneficiaryId;
    public long DisbursedAmount { get; } = disbursedAmount;
    public string Currency { get; } = currency;
    public Guid Tenant { get; } = tenant;

    public static DisbursementCreatedDomainEvent Create(
        Guid aggregateId,
        Guid paymentId,
        Guid beneficiaryId,
        long disbursedAmount,
        string currency,
        Guid tenant,
        Guid? eventId = null,
        Instant? occurredAt = null,
        Dictionary<string, object>? metadata = null)
    {
        return new DisbursementCreatedDomainEvent(
            aggregateId,
            paymentId,
            beneficiaryId,
            disbursedAmount,
            currency,
            tenant,
            eventId ?? Guid.NewGuid(),
            occurredAt ?? SystemClock.Instance.GetCurrentInstant(),
            metadata
        );
    }
}
