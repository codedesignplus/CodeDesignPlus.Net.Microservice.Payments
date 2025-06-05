namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentUpdatedDomainEvent")]
public class PaymentUpdatedDomainEvent(
     Guid aggregateId,
     Guid? eventId = null,
     Instant? occurredAt = null,
     Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public static PaymentUpdatedDomainEvent Create(Guid aggregateId)
    {
        return new PaymentUpdatedDomainEvent(aggregateId);
    }
}
