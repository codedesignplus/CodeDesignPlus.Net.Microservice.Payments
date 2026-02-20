using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentResponseAssociatedDomainEvent", autoCreate: false)]
public class PaymentResponseAssociatedDomainEvent(
    Guid aggregateId,
    string module,
    Guid ReferenceId,
    PaymentStatus status,
    Dictionary<string, string?> response,
    Guid? tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public string Module { get; } = module;
    public Guid ReferenceId { get; } = ReferenceId;
    public PaymentStatus Status { get; } = status;
    public Dictionary<string, string?> Response { get; } = response;
    public Guid? Tenant { get; } = tenant;

    public static PaymentResponseAssociatedDomainEvent Create(Guid aggregateId, string module, Guid referenceId, PaymentStatus status, Dictionary<string, string?> response, Guid? tenant)
    {
        return new PaymentResponseAssociatedDomainEvent(aggregateId, module, referenceId, status, response, tenant);
    }
}
