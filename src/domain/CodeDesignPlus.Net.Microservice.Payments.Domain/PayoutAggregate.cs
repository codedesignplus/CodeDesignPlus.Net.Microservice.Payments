using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

/// <summary>
/// A payment to a third party out of the tenant's collection account, one per request.
/// </summary>
/// <remarks>
/// Its id is the one the calling module generated, so repeating the request does not pay twice. It is closed by the
/// provider's event, never by the request: asking for a payout is not paying it. The provider's batch holds a single
/// transaction, because each request is a payment of its own and is never grouped with others.
/// <para>
/// The provider's events and the reconciliation job will write it at the same time, hence
/// <see cref="IVersionedAggregate"/>.
/// </para>
/// </remarks>
public class PayoutAggregate(Guid id) : AggregateRootBase(id), IVersionedAggregate
{
    /// <inheritdoc/>
    public long Version { get; private set; }

    /// <summary>
    /// Tenant whose collection account pays.
    /// </summary>
    public Guid Tenant { get; private set; }

    /// <summary>
    /// Module that asked for the payout.
    /// </summary>
    public string Module { get; private set; } = null!;

    /// <summary>
    /// Id of what is being paid in the calling module.
    /// </summary>
    public Guid ReferenceId { get; private set; }

    /// <summary>
    /// Why the money leaves the account.
    /// </summary>
    public PayoutPurpose Purpose { get; private set; }

    /// <summary>
    /// Beneficiary registered in this service; its bank account never leaves it.
    /// </summary>
    public Guid BeneficiaryId { get; private set; }

    /// <summary>
    /// Amount to pay, in minor units with its currency.
    /// </summary>
    public Money Amount { get; private set; } = null!;

    /// <summary>
    /// What is being paid, as the module describes it.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// User who asked for the payout, kept for the audit trail.
    /// </summary>
    public Guid RequestedBy { get; private set; }

    /// <summary>
    /// Current state of the payout.
    /// </summary>
    public PayoutStatus Status { get; private set; }

    /// <summary>
    /// Code of the provider that pays it, once sent.
    /// </summary>
    public string? Provider { get; private set; }

    /// <summary>
    /// Id of the provider's batch, once sent.
    /// </summary>
    public string? ProviderBatchId { get; private set; }

    /// <summary>
    /// Id of the transaction inside the provider's batch, once sent.
    /// </summary>
    public string? ProviderTransactionId { get; private set; }

    /// <summary>
    /// Reason given by the provider when the payout failed.
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Registers the request of a payout.
    /// </summary>
    /// <param name="id">Id generated once by the calling module.</param>
    /// <param name="tenant">Tenant whose collection account pays.</param>
    /// <param name="module">Module that asked for the payout.</param>
    /// <param name="referenceId">Id of what is being paid in the calling module.</param>
    /// <param name="purpose">Why the money leaves the account.</param>
    /// <param name="beneficiaryId">Beneficiary registered in this service.</param>
    /// <param name="amount">Amount to pay.</param>
    /// <param name="description">What is being paid.</param>
    /// <param name="requestedBy">User who asked for the payout.</param>
    /// <returns>The payout in <see cref="PayoutStatus.Requested"/>.</returns>
    public static PayoutAggregate Request(Guid id, Guid tenant, string module, Guid referenceId, PayoutPurpose purpose, Guid beneficiaryId, Money amount, string description, Guid requestedBy)
    {
        DomainGuard.GuidIsEmpty(id, Errors.PayoutIdCannotBeEmpty);
        DomainGuard.GuidIsEmpty(tenant, Errors.MerchantAccountTenantCannotBeEmpty);
        DomainGuard.IsNullOrEmpty(module, Errors.ModuleCannotBeNullOrEmpty);
        DomainGuard.GuidIsEmpty(referenceId, Errors.ReferenceIdCannotBeEmpty);
        DomainGuard.IsFalse(Enum.IsDefined(purpose), Errors.PayoutPurposeIsRequired);
        DomainGuard.GuidIsEmpty(beneficiaryId, Errors.PayoutBeneficiaryIsRequired);
        DomainGuard.IsNull(amount, Errors.AmountCannotBeNull);
        DomainGuard.IsTrue(amount.Amount <= 0, Errors.PayoutAmountMustBePositive);
        DomainGuard.IsNullOrEmpty(description, Errors.DescriptionCannotBeNullOrEmpty);
        DomainGuard.GuidIsEmpty(requestedBy, Errors.PayoutRequestedByCannotBeEmpty);

        var aggregate = new PayoutAggregate(id)
        {
            Tenant = tenant,
            Module = module,
            ReferenceId = referenceId,
            Purpose = purpose,
            BeneficiaryId = beneficiaryId,
            Amount = amount,
            Description = description,
            RequestedBy = requestedBy,
            Status = PayoutStatus.Requested,
            IsActive = true,
            CreatedBy = requestedBy,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        aggregate.AddEvent(PayoutRequestedDomainEvent.Create(id, tenant, module, referenceId, purpose, beneficiaryId, amount, requestedBy));

        return aggregate;
    }
}
