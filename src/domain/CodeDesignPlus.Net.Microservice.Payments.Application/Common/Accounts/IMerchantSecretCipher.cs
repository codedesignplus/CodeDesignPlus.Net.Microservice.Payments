using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;

/// <summary>
/// Encrypts and decrypts the secrets of the tenants' merchant accounts with the Vault. Only ciphertext is stored, and
/// the plain secrets live in memory for the one operation that needs them.
/// </summary>
public interface IMerchantSecretCipher
{
    /// <summary>
    /// Encrypts the secrets of a tenant's account, bound to that tenant.
    /// </summary>
    /// <param name="tenant">The tenant that owns the account; a ciphertext of one tenant cannot be decrypted as another's.</param>
    /// <param name="secrets">The secrets in clear.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ciphertext to store in the account.</returns>
    Task<SealedSecrets> SealAsync(Guid tenant, MerchantSecrets secrets, CancellationToken cancellationToken);

    /// <summary>
    /// Decrypts the secrets of a tenant's account for the operation at hand. Never cache nor log the result.
    /// </summary>
    /// <param name="tenant">The tenant that owns the account.</param>
    /// <param name="secrets">The ciphertext stored in the account.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The secrets in clear.</returns>
    Task<MerchantSecrets> OpenAsync(Guid tenant, SealedSecrets secrets, CancellationToken cancellationToken);
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
