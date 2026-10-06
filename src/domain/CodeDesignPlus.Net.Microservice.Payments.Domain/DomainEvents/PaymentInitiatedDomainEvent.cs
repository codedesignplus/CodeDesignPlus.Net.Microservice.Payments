using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// Se inició un cobro en la pasarela.
/// </summary>
/// <remarks>
/// Lleva el tipo de medio de pago y los cuatro últimos dígitos, no el medio de pago entero: ese objeto trae el código
/// de seguridad de la tarjeta, y un evento viaja por el bus y lo puede guardar quien lo consuma. PCI DSS (requisito 3.2)
/// no deja conservarlo después de la autorización (pendings/322).
/// </remarks>
[EventKey<PaymentAggregate>(1, "PaymentInitiatedDomainEvent")]
public class PaymentInitiatedDomainEvent(
    Guid aggregateId,
    string module,
    Money subTotal,
    Money tax,
    Money total,
    Net.ValueObjects.User.Buyer buyer,
    Net.ValueObjects.User.Payer payer,
    string paymentMethodType,
    string? last4Digits,
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
    /// <summary>
    /// El tipo de medio de pago (VISA, PSE…).
    /// </summary>
    public string PaymentMethodType { get; } = paymentMethodType;
    /// <summary>
    /// Los cuatro últimos dígitos de la tarjeta, o nada si no se pagó con tarjeta.
    /// </summary>
    public string? Last4Digits { get; } = last4Digits;
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
            paymentMethod.Type,
            paymentMethod.CreditCard?.Last4Digits,
            description,
            paymentProvider,
            tenant,
            eventId ?? Guid.NewGuid(),
            occurredAt ?? SystemClock.Instance.GetCurrentInstant(),
            metadata
        );
    }
}
