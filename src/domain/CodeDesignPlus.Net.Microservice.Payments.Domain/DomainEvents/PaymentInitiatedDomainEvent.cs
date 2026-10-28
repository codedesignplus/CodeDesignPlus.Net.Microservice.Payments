using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// A payment was started: the payer is sent to the provider's hosted page.
/// </summary>
/// <remarks>
/// Carries no payment method: the payer chooses it on the provider's page, so the platform never sees card data.
/// </remarks>
[EventKey<PaymentAggregate>(1, "PaymentInitiatedDomainEvent")]
public class PaymentInitiatedDomainEvent(
    Guid aggregateId,
    string module,
    Money subTotal,
    Money tax,
    Money total,
    Net.ValueObjects.User.Buyer buyer,
    string description,
    string provider,
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
    public string Description { get; } = description;

    /// <summary>
    /// Code of the provider that processes the payment.
    /// </summary>
    public string Provider { get; } = provider;
    public Guid? Tenant { get; } = tenant;

    public static PaymentInitiatedDomainEvent Create(Guid aggregateId, string module, Money subTotal, Money tax, Money total, Net.ValueObjects.User.Buyer buyer, string description, string provider, Guid? tenant)
    {
        return new PaymentInitiatedDomainEvent(aggregateId, module, subTotal, tax, total, buyer, description, provider, tenant, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());
    }
}
