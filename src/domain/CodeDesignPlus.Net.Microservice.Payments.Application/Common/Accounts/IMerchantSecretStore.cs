using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;

/// <summary>
/// Keeps the secrets of the merchant accounts in the Vault. They are never stored anywhere else.
/// </summary>
public interface IMerchantSecretStore
{
    /// <summary>
    /// Saves the secrets of an account, replacing any previous ones.
    /// </summary>
    /// <param name="accountId">The merchant account.</param>
    /// <param name="secrets">The secrets to keep.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The reference the account keeps to read them again.</returns>
    Task<string> SaveAsync(Guid accountId, MerchantSecrets secrets, CancellationToken cancellationToken);

    /// <summary>
    /// Reads the secrets of an account.
    /// </summary>
    /// <param name="reference">The reference the account keeps.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The secrets, or null when they were deleted.</returns>
    Task<MerchantSecrets?> GetAsync(string reference, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the secrets of an account with every version of them. Deleting what is not there is harmless.
    /// </summary>
    /// <param name="accountId">The merchant account.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(Guid accountId, CancellationToken cancellationToken);
}

/// <summary>
/// The platform's own merchant account, configured for the deployment instead of connected by a tenant.
/// </summary>
public interface IPlatformMerchantAccount
{
    /// <summary>
    /// The credentials of the platform's account.
    /// </summary>
    /// <returns>The credentials, or null when the deployment has no platform account configured.</returns>
    MerchantCredentials? Get();
}
