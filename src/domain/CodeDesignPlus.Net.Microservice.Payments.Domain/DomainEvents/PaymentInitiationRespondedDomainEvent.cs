namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentInitiationRespondedDomainEvent", autoCreate: false)]
public class PaymentInitiationRespondedDomainEvent(
    Guid aggregateId,
    Dictionary<string, string?> response,
    Guid? tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Dictionary<string, string?> Response { get; } = response;
    public Guid? Tenant { get; } = tenant;
    public static PaymentInitiationRespondedDomainEvent Create(Guid aggregateId, Dictionary<string, string?> response, Guid? tenant)
    {
        return new PaymentInitiationRespondedDomainEvent(aggregateId, response, tenant);
    }
}
