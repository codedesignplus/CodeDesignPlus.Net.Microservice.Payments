using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;

/// <summary>
/// Notices when the provider stops accepting the keys of a collection account.
/// </summary>
public interface IMerchantAccountHealth
{
    /// <summary>
    /// Whether an error is the provider rejecting the keys, as opposed to the provider or the Vault not answering.
    /// </summary>
    /// <param name="exception">The error.</param>
    /// <returns>True when the keys were rejected.</returns>
    bool IsRejection(CodeDesignPlusException exception);

    /// <summary>
    /// Counts a rejection of the keys of the account that collected a payment; repeated, it invalidates the account.
    /// </summary>
    /// <param name="payment">The payment whose account was rejected.</param>
    /// <param name="reason">What was rejected; never a secret.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ReportRejectionAsync(PaymentAggregate payment, string reason, CancellationToken cancellationToken);

    /// <summary>
    /// The keys worked again; earlier rejections were transient. Writes only when there were rejections to forget.
    /// </summary>
    /// <param name="payment">The payment whose account answered.</param>
    /// <param name="credentials">The credentials that worked.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ReportAcceptanceAsync(PaymentAggregate payment, MerchantCredentials credentials, CancellationToken cancellationToken);
}

/// <summary>
/// Invalidates a collection account only after <see cref="PaymentProvidersOptions.InvalidateAfterRejections"/>
/// rejections in a row, so a single transient failure never stops a tenant from charging online.
/// </summary>
/// <remarks>
/// The platform's own account is configuration, not an aggregate: its rejections are logged as errors for the
/// operators, who are the only ones who can fix it.
/// </remarks>
public sealed class MerchantAccountHealth(IMerchantAccountRepository repository, IMerchantSecretsCache cache, IPubSub pubsub, IOptions<PaymentProvidersOptions> options, ILogger<MerchantAccountHealth> logger) : IMerchantAccountHealth
{
    /// <inheritdoc/>
    public bool IsRejection(CodeDesignPlusException exception) => exception.Code == Errors.MerchantKeysRejectedByProvider.Code;

    /// <inheritdoc/>
    public async Task ReportRejectionAsync(PaymentAggregate payment, string reason, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payment);

        if (payment.PlatformAccount || payment.Tenant is not { } tenant)
        {
            logger.LogError("The provider rejected the keys of the platform's collection account while handling payment {PaymentId}: {Reason}", payment.Id, reason);

            return;
        }

        logger.LogWarning("The provider rejected the keys of the collection account of tenant {Tenant} while handling payment {PaymentId}: {Reason}", tenant, payment.Id, reason);

        MerchantAccountAggregate saved = null!;

        var written = await repository.UpdateWithRetryAsync(async () => (await repository.GetByTenantAsync(tenant, cancellationToken))!, account =>
        {
            saved = account;

            return account.RegisterRejection(reason, options.Value.InvalidateAfterRejections);
        }, cancellationToken);

        if (!written || saved.Status != MerchantAccountStatus.Invalid)
        {
            return;
        }

        cache.Forget(saved.Id);

        await pubsub.PublishAsync(saved.GetAndClearEvents(), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ReportAcceptanceAsync(PaymentAggregate payment, MerchantCredentials credentials, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(credentials);

        if (credentials.PendingRejections == 0 || payment.PlatformAccount || payment.Tenant is not { } tenant)
        {
            return;
        }

        await repository.UpdateWithRetryAsync(async () => (await repository.GetByTenantAsync(tenant, cancellationToken))!, account => account.RegisterAcceptance(), cancellationToken);
    }
}
