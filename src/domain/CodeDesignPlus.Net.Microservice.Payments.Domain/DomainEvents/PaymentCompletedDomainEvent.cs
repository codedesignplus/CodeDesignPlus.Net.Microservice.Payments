using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentCompletedDomainEvent")]
public class PaymentCompletedDomainEvent(
    Guid aggregateId,
    Provider provider,
    Transaction transaction,
    object request,
    object response,
    Guid? tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Provider Provider { get; } = provider;
    public Transaction Transaction { get; } = transaction;
    public object Request { get; } = request;
    public object Response { get; } = response;
    public Guid? Tenant { get; } = tenant;
    
    public static PaymentCompletedDomainEvent Create(Guid aggregateId, Provider provider, Transaction transaction, object request, object response, Guid? tenant, Guid? eventId = null, Instant? occurredAt = null, Dictionary<string, object>? metadata = null)
    {
        return new PaymentCompletedDomainEvent(aggregateId, provider, transaction, request, response, tenant, eventId, occurredAt, metadata);
    }
}
