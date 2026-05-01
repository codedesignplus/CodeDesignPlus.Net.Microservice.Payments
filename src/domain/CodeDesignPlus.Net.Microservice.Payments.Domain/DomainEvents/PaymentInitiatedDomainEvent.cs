using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<PaymentAggregate>(1, "PaymentInitiatedDomainEvent")]
public class PaymentInitiatedDomainEvent(
    Guid aggregateId,
    string module,
    Money subTotal,
    Money tax,
    Money total,
    Net.ValueObjects.User.Buyer buyer,
    Net.ValueObjects.User.Payer payer,
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
    public Money SubTotal { get; } = subTotal;
    public Money Tax { get; } = tax;
    public Money Total { get; } = total;
    public Net.ValueObjects.User.Buyer Buyer { get; } = buyer;
    public Net.ValueObjects.User.Payer Payer { get; } = payer;
    public PaymentMethod PaymentMethod { get; } = paymentMethod;
    public string Description { get; } = description;
    public PaymentProvider Provider { get; } = paymentProvider;
    public Guid? Tenant { get; } = tenant;

    public static PaymentInitiatedDomainEvent Create(Guid aggregateId, 
        string module,
        Money subTotal,
        Money tax,
        Money total,
        Net.ValueObjects.User.Buyer buyer,
        Net.ValueObjects.User.Payer payer,
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
            buyer,
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
