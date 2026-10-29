using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// The payment reached its final outcome.
/// </summary>
/// <remarks>
/// It is the contract with every module that charges through this service, and they read <c>Module</c>,
/// <c>ReferenceId</c>, <c>Status</c> (as a number) and <c>Tenant</c>. It only grows: <c>CapturedAmount</c>,
/// <c>Provider</c>, the commission (<c>ProviderFee</c>, <c>ProviderFeeTax</c>, <c>ExpectedNetAmount</c>,
/// <c>FeeSource</c>) and <c>PlatformAccount</c> are additions that a consumer which does not know them ignores.
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
    Money? providerFee = null,
    Money? providerFeeTax = null,
    Money? expectedNetAmount = null,
    string? feeSource = null,
    bool platformAccount = false,
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

    /// <summary>
    /// The commission the provider keeps from the captured amount, without its tax, in minor units. Null when the
    /// payment did not succeed or the commission could not be known.
    /// </summary>
    public Money? ProviderFee { get; } = providerFee;

    /// <summary>
    /// The tax on <see cref="ProviderFee"/>, in minor units.
    /// </summary>
    public Money? ProviderFeeTax { get; } = providerFeeTax;

    /// <summary>
    /// What the merchant should receive: the captured amount less the commission and its tax.
    /// </summary>
    public Money? ExpectedNetAmount { get; } = expectedNetAmount;

    /// <summary>
    /// Whether the commission was <c>Calculated</c> with the account's rate or <c>Reported</c> by the provider.
    /// </summary>
    public string? FeeSource { get; } = feeSource;

    /// <summary>
    /// Whether the payment went to the platform's own merchant account instead of the tenant's: its commission is the
    /// platform's, not the tenant's.
    /// </summary>
    public bool PlatformAccount { get; } = platformAccount;

    public static PaymentResponseAssociatedDomainEvent Create(Guid aggregateId, string module, Guid referenceId, PaymentStatus status, Dictionary<string, string?> response, Guid? tenant, Money? capturedAmount, string? provider, Money? providerFee = null, Money? providerFeeTax = null, Money? expectedNetAmount = null, string? feeSource = null, bool platformAccount = false)
    {
        return new PaymentResponseAssociatedDomainEvent(aggregateId, module, referenceId, status, response, tenant, capturedAmount, provider, providerFee, providerFeeTax, expectedNetAmount, feeSource, platformAccount);
    }
}
