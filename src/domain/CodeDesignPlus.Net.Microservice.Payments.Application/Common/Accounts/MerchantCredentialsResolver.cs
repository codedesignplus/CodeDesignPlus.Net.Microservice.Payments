using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;

/// <summary>
/// Resolves which merchant account acts in an operation and reads its secrets.
/// </summary>
public interface IMerchantCredentialsResolver
{
    /// <summary>
    /// The credentials to collect a new payment with.
    /// </summary>
    /// <param name="tenant">Tenant that charges; ignored when <paramref name="platformAccount"/> is set.</param>
    /// <param name="platformAccount">Whether the payment goes to the platform's own account.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The credentials of an account that can charge.</returns>
    /// <exception cref="CodeDesignPlus.Net.Exceptions.CodeDesignPlusException">Thrown when no account can charge.</exception>
    Task<MerchantCredentials> ForNewPaymentAsync(Guid tenant, bool platformAccount, CancellationToken cancellationToken);

    /// <summary>
    /// The credentials of the account that collected a payment, to trust its events or ask about it.
    /// </summary>
    /// <param name="payment">The payment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The credentials, or null when the account no longer has keys.</returns>
    Task<MerchantCredentials?> ForPaymentAsync(PaymentAggregate payment, CancellationToken cancellationToken);
}

/// <summary>
/// Resolves the merchant account of an operation: the platform's, from the configuration, or the tenant's, whose
/// secrets are decrypted by the Vault in one batch and kept in this process's memory for a short while.
/// </summary>
public sealed class MerchantCredentialsResolver(IMerchantAccountRepository repository, IMerchantSecretCipher cipher, IMerchantSecretsCache cache, IPlatformMerchantAccount platformAccount) : IMerchantCredentialsResolver
{
    /// <inheritdoc/>
    public async Task<MerchantCredentials> ForNewPaymentAsync(Guid tenant, bool platformAccount, CancellationToken cancellationToken)
    {
        if (platformAccount)
        {
            var platform = PlatformAccount();

            ApplicationGuard.IsNull(platform, Errors.PlatformAccountIsNotConfigured);

            return platform!;
        }

        ApplicationGuard.GuidIsEmpty(tenant, Errors.TenantIsRequiredToCharge);

        var account = await repository.GetByTenantAsync(tenant, cancellationToken);

        ApplicationGuard.IsTrue(account is null || account.Status != MerchantAccountStatus.Active, Errors.MerchantAccountIsNotActive);

        var credentials = await ReadAsync(account!, cancellationToken);

        ApplicationGuard.IsNull(credentials, Errors.MerchantAccountIsNotActive);

        return credentials!;
    }

    /// <inheritdoc/>
    public async Task<MerchantCredentials?> ForPaymentAsync(PaymentAggregate payment, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payment);

        if (payment.PlatformAccount)
        {
            return PlatformAccount();
        }

        if (payment.Tenant is not { } tenant)
        {
            return null;
        }

        var account = await repository.GetByTenantAsync(tenant, cancellationToken);

        return account is null ? null : await ReadAsync(account, cancellationToken);
    }

    private MerchantCredentials? PlatformAccount() => platformAccount.Get();

    private async Task<MerchantCredentials?> ReadAsync(MerchantAccountAggregate account, CancellationToken cancellationToken)
    {
        if (account.Secrets is null || account.Environment is not { } environment || string.IsNullOrEmpty(account.Provider) || string.IsNullOrEmpty(account.PublicKey))
        {
            return null;
        }

        var secrets = cache.Get(account.Id, account.Version);

        if (secrets is null)
        {
            secrets = await cipher.OpenAsync(account.Tenant, account.Secrets, cancellationToken);

            cache.Set(account.Id, account.Version, secrets);
        }

        return new MerchantCredentials(account.Provider, environment, account.PublicKey, secrets, account.PayoutsSourceAccountId) { PendingRejections = account.ConsecutiveRejections };
    }
}
