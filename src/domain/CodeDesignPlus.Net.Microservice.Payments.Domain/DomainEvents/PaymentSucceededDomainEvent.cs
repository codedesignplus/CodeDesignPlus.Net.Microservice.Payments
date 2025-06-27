using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentSucceededDomainEvent")]
public class PaymentSucceededDomainEvent(
    Guid aggregateId,
    string? providerTransactionId,
    string? providerMessage,
    Payer payer,
    Guid? tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public string? ProviderTransactionId { get; } = providerTransactionId;
    public string? ProviderMessage { get; } = providerMessage;
    public Payer Payer { get; } = payer;
    public Guid? Tenant { get; } = tenant;
    public static PaymentSucceededDomainEvent Create(Guid aggregateId, string? providerTransactionId, string? providerMessage, Payer payer, Guid? tenant)
    {
        return new PaymentSucceededDomainEvent(aggregateId, providerTransactionId, providerMessage, payer,  tenant);
    }
}
