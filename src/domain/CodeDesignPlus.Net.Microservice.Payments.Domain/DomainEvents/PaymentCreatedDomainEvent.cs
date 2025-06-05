namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentCreatedDomainEvent")]
public class PaymentCreatedDomainEvent(
     Guid aggregateId,
     Guid? eventId = null,
     Instant? occurredAt = null,
     Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public static PaymentCreatedDomainEvent Create(Guid aggregateId)
    {
        return new PaymentCreatedDomainEvent(aggregateId);
    }
}
