using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentSucceededDomainEvent")]
public class PaymentResponseAssociatedDomainEvent(
    Guid aggregateId,
    PaymentStatus status,
    string providerResponseMessage,
    FinancialNetwork financialNetwork,
    Guid? tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    
    public PaymentStatus Status { get; } = status;
    public string ProviderResponseMessage { get; } = providerResponseMessage;
    public FinancialNetwork FinancialNetwork { get; } = financialNetwork;
    public Guid? Tenant { get; } = tenant;

    public static PaymentResponseAssociatedDomainEvent Create(Guid aggregateId, 
        PaymentStatus status, 
        string providerResponseMessage, 
        FinancialNetwork financialNetwork, 
        Guid? tenant)
    {
        return new PaymentResponseAssociatedDomainEvent(aggregateId, status, providerResponseMessage, financialNetwork, tenant);
    }
}
