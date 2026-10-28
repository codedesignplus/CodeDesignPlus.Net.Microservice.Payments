using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;

/// <summary>
/// Keeps the decrypted secrets of an account in this process's memory for a short while, so the peaks at the turn of
/// the month do not send one decryption to the Vault per payment and per event.
/// </summary>
/// <remarks>
/// Only in memory: never in a distributed cache, never in a log. Entries are keyed by account and version of the
/// aggregate, so keys replaced in another process stop being used here as soon as the new version is read.
/// </remarks>
public interface IMerchantSecretsCache
{
    /// <summary>
    /// The secrets of an account at a version, if they are still cached.
    /// </summary>
    /// <param name="accountId">The merchant account.</param>
    /// <param name="version">The version of the account the secrets were decrypted from.</param>
    /// <returns>The secrets, or null.</returns>
    MerchantSecrets? Get(Guid accountId, long version);

    /// <summary>
    /// Caches the secrets of an account at a version, dropping those of any earlier version.
    /// </summary>
    /// <param name="accountId">The merchant account.</param>
    /// <param name="version">The version of the account the secrets were decrypted from.</param>
    /// <param name="secrets">The decrypted secrets.</param>
    void Set(Guid accountId, long version, MerchantSecrets secrets);

    /// <summary>
    /// Forgets the secrets of an account: its keys were replaced, disconnected or purged.
    /// </summary>
    /// <param name="accountId">The merchant account.</param>
    void Forget(Guid accountId);
}
