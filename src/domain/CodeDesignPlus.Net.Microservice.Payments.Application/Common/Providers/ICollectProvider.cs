using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;

/// <summary>
/// A payment provider, identified by its code. Each capability is a separate interface that a provider implements
/// only if it can.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Code of the provider, as it is configured and stored (for example "Wompi").
    /// </summary>
    string Code { get; }
}

/// <summary>
/// Collects payments: the mandatory capability of every provider.
/// </summary>
/// <remarks>
/// The payer always continues on the provider's hosted page, so card data never reaches this service.
/// </remarks>
public interface ICollectProvider : IPaymentProvider
{
    /// <summary>
    /// Builds the hosted page where the payer finishes the payment.
    /// </summary>
    /// <param name="payment">The payment to collect; its id is the reference the provider reports back.</param>
    /// <param name="credentials">The merchant account that collects.</param>
    /// <param name="returnUrl">Page the provider returns to when the payer finishes.</param>
    /// <param name="expiresAt">When the hosted page stops accepting the payment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Where to send the payer.</returns>
    Task<CheckoutSession> CreateCheckoutAsync(PaymentAggregate payment, MerchantCredentials credentials, Uri returnUrl, Instant expiresAt, CancellationToken cancellationToken);

    /// <summary>
    /// Reads an event the provider posted to the webhook, without trusting it yet.
    /// </summary>
    /// <param name="body">The raw body of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The reference and outcome it claims, or null when it is not a collection event this adapter knows.</returns>
    Task<ProviderNotification?> ReadNotificationAsync(string body, CancellationToken cancellationToken);

    /// <summary>
    /// Checks the signature of an event against the secret of the account that the event's reference points to.
    /// </summary>
    /// <param name="body">The raw body of the request.</param>
    /// <param name="credentials">The merchant account of the payment the event refers to.</param>
    /// <returns>Whether the event was signed by that merchant.</returns>
    bool IsAuthentic(string body, MerchantCredentials credentials);

    /// <summary>
    /// Asks the provider how the payment ended, for when the payer comes back or an event was lost.
    /// </summary>
    /// <param name="payment">The payment to look up by its reference.</param>
    /// <param name="credentials">The merchant account that collected it.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome, or null when the provider has no transaction for it yet.</returns>
    Task<CollectOutcome?> GetOutcomeAsync(PaymentAggregate payment, MerchantCredentials credentials, CancellationToken cancellationToken);
}

/// <summary>
/// Where to send the payer to finish a payment.
/// </summary>
/// <param name="RedirectUrl">The provider's hosted page, already signed.</param>
public sealed record CheckoutSession(Uri RedirectUrl);

/// <summary>
/// A collection event as the provider posted it, before its signature is checked.
/// </summary>
/// <param name="Reference">The reference of the payment, which is its id in this service.</param>
/// <param name="Outcome">What the event says happened.</param>
public sealed record ProviderNotification(string Reference, CollectOutcome Outcome);

/// <summary>
/// The outcome of a payment, normalized from whatever the provider reports.
/// </summary>
/// <param name="Status">Normalized status; <see cref="PaymentStatus.Pending"/> is not final.</param>
/// <param name="TransactionId">Id of the transaction at the provider.</param>
/// <param name="Amount">What the provider charged, in minor units of the payment's currency.</param>
/// <param name="PaymentMethodType">Payment method the payer chose on the provider's page.</param>
/// <param name="Raw">What the provider reported, kept as the record of the outcome. Never contains secrets.</param>
public sealed record CollectOutcome(PaymentStatus Status, string? TransactionId, Money? Amount, string? PaymentMethodType, Dictionary<string, string?> Raw)
{
    /// <summary>
    /// The commission the provider says it keeps from this transaction, when it reports it; it prevails over the one
    /// computed with the account's rates. Wompi does not report it in the transaction (pendings/660).
    /// </summary>
    public ProviderFee? ReportedFee { get; init; }

    /// <summary>
    /// Whether the status is one the payment can close with.
    /// </summary>
    public bool IsFinal => Status is PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Expired;
}
