using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;

/// <summary>
/// Refunds of collected payments. For now only their events are read: refunds are asked for on the provider's panel,
/// and the flow that asks for them from here is a later stage (pendings/660).
/// </summary>
/// <remarks>
/// The events arrive at the same webhook as the payments and are signed the same way, so they are checked with
/// <see cref="ICollectProvider.IsAuthentic"/>.
/// </remarks>
public interface IRefundProvider : IPaymentProvider
{
    /// <summary>
    /// Reads a refund event the provider posted to the webhook, without trusting it yet.
    /// </summary>
    /// <param name="body">The raw body of the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What the event claims, or null when it is not a refund event this adapter knows.</returns>
    Task<ProviderRefundNotification?> ReadRefundNotificationAsync(string body, CancellationToken cancellationToken);
}

/// <summary>
/// A refund event as the provider posted it, before its signature is checked.
/// </summary>
/// <param name="Reference">The reference the event carries, which may be the id of the payment; null when it has none.</param>
/// <param name="TransactionId">Id at the provider of the transaction refunded.</param>
/// <param name="RefundId">Id of the refund at the provider.</param>
/// <param name="Status">Its normalized status.</param>
/// <param name="Amount">What is refunded, when the provider says it.</param>
/// <param name="Code">Code of the provider's reason, when it did not go through.</param>
/// <param name="Message">The provider's reason in its own words, when it did not go through.</param>
public sealed record ProviderRefundNotification(string? Reference, string? TransactionId, string RefundId, RefundStatus Status, Money? Amount, string? Code, string? Message);
