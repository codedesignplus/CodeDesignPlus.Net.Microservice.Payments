using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands;

/// <summary>
/// The keys a tenant pastes to connect its collection account.
/// </summary>
/// <param name="PublicKey">The merchant's public key.</param>
/// <param name="PrivateKey">The merchant's private key.</param>
/// <param name="EventsSecret">The secret the provider signs its events with.</param>
/// <param name="IntegritySecret">The secret that signs the hosted page.</param>
/// <param name="PayoutsPrincipalId">User principal of the payouts API, when payouts are connected.</param>
/// <param name="PayoutsApiKey">Key of the payouts API, when payouts are connected.</param>
/// <param name="PayoutsSourceAccountId">Provider account the payouts are paid from, when payouts are connected.</param>
public sealed record MerchantKeys(string PublicKey, string PrivateKey, string EventsSecret, string IntegritySecret, string? PayoutsPrincipalId, string? PayoutsApiKey, string? PayoutsSourceAccountId)
{
    /// <summary>
    /// Never prints a secret.
    /// </summary>
    /// <returns>A redacted description.</returns>
    public override string ToString() => $"{nameof(MerchantKeys)} {{ PublicKey = {PublicKey} }}";
}

/// <summary>
/// Validates the keys a tenant pastes and stores their secrets in the Vault.
/// </summary>
public interface IMerchantAccountConnector
{
    /// <summary>
    /// Validates the keys against the provider of the account and stores their secrets.
    /// </summary>
    /// <param name="account">The account the keys are for; it must already have its provider.</param>
    /// <param name="keys">The keys the tenant pasted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What the account keeps of the keys.</returns>
    /// <exception cref="CodeDesignPlus.Net.Exceptions.CodeDesignPlusException">Thrown when the keys do not work for this deployment.</exception>
    Task<MerchantConnection> ValidateAndStoreAsync(MerchantAccountAggregate account, MerchantKeys keys, CancellationToken cancellationToken);
}

/// <summary>
/// Validates the keys before anything is stored: a tenant that pastes the wrong ones keeps the ones that worked.
/// </summary>
public sealed class MerchantAccountConnector(IPaymentProviderFactory providers, IMerchantSecretStore secretStore, IOptions<PaymentProvidersOptions> options, ILogger<MerchantAccountConnector> logger) : IMerchantAccountConnector
{
    /// <inheritdoc/>
    public async Task<MerchantConnection> ValidateAndStoreAsync(MerchantAccountAggregate account, MerchantKeys keys, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(keys);

        ApplicationGuard.IsNullOrEmpty(account.Provider, Errors.NoPaymentProviderForCountry);

        var payoutsFields = new[] { keys.PayoutsPrincipalId, keys.PayoutsApiKey, keys.PayoutsSourceAccountId };
        var withPayouts = payoutsFields.All(x => !string.IsNullOrWhiteSpace(x));

        ApplicationGuard.IsTrue(!withPayouts && payoutsFields.Any(x => !string.IsNullOrWhiteSpace(x)), Errors.PayoutsCredentialsAreIncomplete);

        if (withPayouts)
        {
            // Fails when the provider cannot pay third parties, before any key is stored.
            providers.GetPayouts(account.Provider!);
        }

        var secrets = new MerchantSecrets(keys.PrivateKey.Trim(), keys.EventsSecret.Trim(), keys.IntegritySecret.Trim(), withPayouts ? keys.PayoutsPrincipalId!.Trim() : null, withPayouts ? keys.PayoutsApiKey!.Trim() : null);
        var publicKey = keys.PublicKey.Trim();

        var validation = await providers.GetCredentials(account.Provider!).ValidateAsync(publicKey, secrets, cancellationToken);

        if (!validation.IsValid)
        {
            logger.LogWarning("The keys of the collection account of tenant {Tenant} were rejected by {Provider}: {Reason}", account.Tenant, account.Provider, validation.Reason);
        }

        ApplicationGuard.IsFalse(validation.IsValid, Errors.MerchantKeysWereRejected);
        ApplicationGuard.IsTrue(validation.Environment != options.Value.Environment, Errors.MerchantKeysEnvironmentMismatch);

        var reference = await secretStore.SaveAsync(account.Id, secrets, cancellationToken);

        return new MerchantConnection(
            validation.Environment!.Value,
            publicKey,
            validation.MerchantName,
            reference,
            MerchantSecrets.HintOf(secrets.PrivateKey),
            MerchantSecrets.HintOf(secrets.EventsSecret),
            MerchantSecrets.HintOf(secrets.IntegritySecret),
            withPayouts ? keys.PayoutsSourceAccountId!.Trim() : null,
            withPayouts ? MerchantSecrets.HintOf(secrets.PayoutsApiKey!) : null);
    }
}
