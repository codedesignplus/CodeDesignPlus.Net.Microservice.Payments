using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentInitiatedDomainEvent", autoCreate: false)]
public class PaymentInitiatedDomainEvent(
    Guid aggregateId,
    string module,
    Amount subTotal,
    Amount tax,
    Amount total,
    Payer payer,
    PaymentMethod paymentMethod,
    string description,
    PaymentProvider paymentProvider,
    Guid? tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public string Module { get; } = module;
    public Amount SubTotal { get; } = subTotal;
    public Amount Tax { get; } = tax;
    public Amount Total { get; } = total;
    public Payer Payer { get; } = payer;
    public PaymentMethod PaymentMethod { get; } = paymentMethod;
    public string Description { get; } = description;
    public PaymentProvider Provider { get; } = paymentProvider;
    public Guid? Tenant { get; } = tenant;

    public static PaymentInitiatedDomainEvent Create(Guid aggregateId, 
        string module,
        Amount subTotal,
        Amount tax,
        Amount total,
        Payer payer,
        PaymentMethod paymentMethod,
        string description,
        PaymentProvider paymentProvider,
        Guid? tenant = null,
        Guid? eventId = null,
        Instant? occurredAt = null,
        Dictionary<string, object>? metadata = null)
    {
        return new PaymentInitiatedDomainEvent(
            aggregateId,
            module,
            subTotal,
            tax,
            total,
            payer,
            paymentMethod,
            description,
            paymentProvider,
            tenant,
            eventId ?? Guid.NewGuid(),
            occurredAt ?? SystemClock.Instance.GetCurrentInstant(),
            metadata
        );
    }
}
