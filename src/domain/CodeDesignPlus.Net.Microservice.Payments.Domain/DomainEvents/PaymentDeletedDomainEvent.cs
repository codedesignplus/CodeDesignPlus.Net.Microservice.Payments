namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentDeletedDomainEvent")]
public class PaymentDeletedDomainEvent(
     Guid aggregateId,
     Guid? eventId = null,
     Instant? occurredAt = null,
     Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public static PaymentDeletedDomainEvent Create(Guid aggregateId)
    {
        return new PaymentDeletedDomainEvent(aggregateId);
    }
}
