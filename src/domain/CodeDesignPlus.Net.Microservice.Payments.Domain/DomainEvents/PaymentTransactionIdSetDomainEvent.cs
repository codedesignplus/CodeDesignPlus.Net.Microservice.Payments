namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "TransactionIdAssignedDomainEvent")]
public class TransactionIdAssignedDomainEvent(
     Guid aggregateId,
     string providerTransactionId,
     Guid? eventId = null,
     Instant? occurredAt = null,
     Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public string ProviderTransactionId { get; } = providerTransactionId;
    public static TransactionIdAssignedDomainEvent Create(Guid aggregateId, string providerTransactionId)
    {
        return new TransactionIdAssignedDomainEvent(aggregateId, providerTransactionId);
    }
}
