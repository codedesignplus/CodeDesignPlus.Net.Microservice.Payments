namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// The provider declined an attempt of the payment, and the payer can still retry on the same hosted page until
/// <see cref="RetryWindowEndsAt"/> (pendings/622).
/// </summary>
/// <remarks>
/// It is not the outcome of the payment: <see cref="PaymentResponseAssociatedDomainEvent"/> still comes, with
/// <c>Succeeded</c> if a retry is approved inside the window or with <c>Failed</c> when the window ends without one. A
/// module tells the payer about the rejection with this event and applies the consequences (cancelling what waited for the
/// payment) only with the final outcome. Wompi lets the payer retry for three minutes with another payment method and the
/// same reference (docs/wompi/reintento-de-pago.md).
/// </remarks>
[EventKey<PaymentAggregate>(1, "PaymentAttemptDeclinedDomainEvent")]
public class PaymentAttemptDeclinedDomainEvent(
    Guid aggregateId,
    string module,
    Guid referenceId,
    Guid? tenant,
    string? provider,
    bool platformAccount,
    PaymentFailureReason failureReason,
    string? providerMessage,
    Instant retryWindowEndsAt,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    /// <summary>
    /// Module that charges through the payment.
    /// </summary>
    public string Module { get; } = module;

    /// <summary>
    /// Id of what the module charges for (a document, a booking, a reservation).
    /// </summary>
    public Guid ReferenceId { get; } = referenceId;

    /// <summary>
    /// Tenant of the payment; null for a platform payment bought before its tenant exists.
    /// </summary>
    public Guid? Tenant { get; } = tenant;

    /// <summary>
    /// Code of the provider that declined the attempt.
    /// </summary>
    public string? Provider { get; } = provider;

    /// <summary>
    /// Whether the payment goes to the platform's own merchant account.
    /// </summary>
    public bool PlatformAccount { get; } = platformAccount;

    /// <summary>
    /// Why the attempt did not succeed, as a number of <see cref="PaymentFailureReason"/>.
    /// </summary>
    public PaymentFailureReason FailureReason { get; } = failureReason;

    /// <summary>
    /// What the bank or the provider said about the attempt, as they said it.
    /// </summary>
    public string? ProviderMessage { get; } = providerMessage;

    /// <summary>
    /// Until when the payer can retry on the provider's page; after it, the payment closes as failed if no retry was approved.
    /// </summary>
    public Instant RetryWindowEndsAt { get; } = retryWindowEndsAt;

    /// <summary>
    /// Creates the event.
    /// </summary>
    /// <param name="aggregateId">Id of the payment.</param>
    /// <param name="module">Module that charges through the payment.</param>
    /// <param name="referenceId">Id of what the module charges for.</param>
    /// <param name="tenant">Tenant of the payment.</param>
    /// <param name="provider">Code of the provider.</param>
    /// <param name="platformAccount">Whether the payment goes to the platform's merchant account.</param>
    /// <param name="failureReason">Why the attempt did not succeed.</param>
    /// <param name="providerMessage">What the bank or the provider said.</param>
    /// <param name="retryWindowEndsAt">Until when the payer can retry.</param>
    /// <returns>The event.</returns>
    public static PaymentAttemptDeclinedDomainEvent Create(Guid aggregateId, string module, Guid referenceId, Guid? tenant, string? provider, bool platformAccount, PaymentFailureReason failureReason, string? providerMessage, Instant retryWindowEndsAt)
    {
        return new PaymentAttemptDeclinedDomainEvent(aggregateId, module, referenceId, tenant, provider, platformAccount, failureReason, providerMessage, retryWindowEndsAt);
    }
}
