using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// The payment reached its final outcome.
/// </summary>
/// <remarks>
/// It is the contract with every module that charges through this service, and they read <c>Module</c>,
/// <c>ReferenceId</c>, <c>Status</c> (as a number) and <c>Tenant</c>. It only grows: <c>CapturedAmount</c> and
/// <c>Provider</c> are additions that a consumer which does not know them ignores.
/// </remarks>
[EventKey<PaymentAggregate>(1, "PaymentResponseAssociatedDomainEvent")]
public class PaymentResponseAssociatedDomainEvent(
    Guid aggregateId,
    string module,
    Guid ReferenceId,
    PaymentStatus status,
    Dictionary<string, string?> response,
    Guid? tenant,
    Money? capturedAmount = null,
    string? provider = null,
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

    /// <summary>
    /// What the provider actually captured, in minor units. Only set when the payment succeeded.
    /// </summary>
    public Money? CapturedAmount { get; } = capturedAmount;

    /// <summary>
    /// Code of the provider that processed the payment.
    /// </summary>
    public string? Provider { get; } = provider;

    public static PaymentResponseAssociatedDomainEvent Create(Guid aggregateId, string module, Guid referenceId, PaymentStatus status, Dictionary<string, string?> response, Guid? tenant, Money? capturedAmount, string? provider)
    {
        return new PaymentResponseAssociatedDomainEvent(aggregateId, module, referenceId, status, response, tenant, capturedAmount, provider);
    }
}
