using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentResponseAssociatedDomainEvent")]
public class PaymentResponseAssociatedDomainEvent(
    Guid aggregateId,
    PaymentStatus status,
    Dictionary<string, string> response,
    Guid? tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{

    public PaymentStatus Status { get; } = status;
    public Dictionary<string, string> Response { get; } = response;
    public Guid? Tenant { get; } = tenant;

    public static PaymentResponseAssociatedDomainEvent Create(Guid aggregateId, PaymentStatus status, Dictionary<string, string> response, Guid? tenant)
    {
        return new PaymentResponseAssociatedDomainEvent(aggregateId, status, response, tenant);
    }
}
