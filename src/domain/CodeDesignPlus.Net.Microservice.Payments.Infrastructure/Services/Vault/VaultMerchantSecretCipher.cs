using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Exceptions.Extensions;
using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.Vault.Abstractions;
using CodeDesignPlus.Net.Vault.Abstractions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Vault;

/// <summary>
/// Encrypts the secrets of the merchant accounts with the Vault's Transit engine, the way ms-microsoftgraph keeps the
/// temporary passwords.
/// </summary>
/// <remarks>
/// The context is the one configured in <c>Vault:Transit:SecretContexts:merchant_account</c> joined to the tenant id:
/// a ciphertext copied from one tenant's account to another's does not decrypt. Every account is encrypted with the same
/// key, <c>PaymentProviders:SecretsKeyName</c>, which the SDK creates derived on first use and refuses if it exists
/// without derivation; a key per encryption would leave tens of thousands of keys in the Vault, orphaned at every
/// rotation. The secrets of an account go in one batch.
/// <para>
/// A Vault that does not answer is reported as <see cref="Errors.MerchantSecretsVaultIsUnavailable"/>, which the
/// entrypoints answer as unavailable so the caller retries; nothing falls back to secrets in clear.
/// </para>
/// </remarks>
public sealed class VaultMerchantSecretCipher(IVaultTransit transit, IOptions<VaultOptions> options, IOptions<PaymentProvidersOptions> providersOptions, ILogger<VaultMerchantSecretCipher> logger) : IMerchantSecretCipher
{
    /// <summary>
    /// Name of the secret context in <c>Vault:Transit:SecretContexts</c>.
    /// </summary>
    public const string SecretContextName = "merchant_account";

    /// <inheritdoc/>
    public async Task<SealedSecrets> SealAsync(Guid tenant, MerchantSecrets secrets, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(secrets);

        var plain = new List<string> { secrets.PrivateKey, secrets.EventsSecret, secrets.IntegritySecret };

        if (secrets.HasPayouts)
        {
            plain.Add(secrets.PayoutsPrincipalId!);
            plain.Add(secrets.PayoutsApiKey!);
        }

        var context = ContextOf(tenant);

        var ciphertexts = await CallVaultAsync(() => transit.EncryptAsync(providersOptions.Value.SecretsKeyName, plain, context), tenant);

        return new SealedSecrets(ciphertexts[0], ciphertexts[1], ciphertexts[2], secrets.HasPayouts ? ciphertexts[3] : null, secrets.HasPayouts ? ciphertexts[4] : null);
    }

    /// <inheritdoc/>
    public async Task<MerchantSecrets> OpenAsync(Guid tenant, SealedSecrets secrets, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(secrets);

        var hasPayouts = secrets.PayoutsPrincipalId is not null && secrets.PayoutsApiKey is not null;

        var ciphertexts = new List<string> { secrets.PrivateKey, secrets.EventsSecret, secrets.IntegritySecret };

        if (hasPayouts)
        {
            ciphertexts.Add(secrets.PayoutsPrincipalId!);
            ciphertexts.Add(secrets.PayoutsApiKey!);
        }

        var context = ContextOf(tenant);

        var plain = await CallVaultAsync(() => transit.DecryptAsync(providersOptions.Value.SecretsKeyName, ciphertexts, context), tenant);

        return new MerchantSecrets(plain[0], plain[1], plain[2], hasPayouts ? plain[3] : null, hasPayouts ? plain[4] : null);
    }

    private async Task<T> CallVaultAsync<T>(Func<Task<T>> call, Guid tenant)
    {
        try
        {
            return await call();
        }
        catch (Exception exception) when (exception is not CodeDesignPlusException)
        {
            // Only the type and the tenant: the message of a Vault error may echo what it was given.
            logger.LogWarning("The Vault did not encrypt or decrypt the keys of the collection account of tenant {Tenant}: {Error}", tenant, exception.GetType().Name);

            throw new CodeDesignPlusException(Layer.Infrastructure, Errors.MerchantSecretsVaultIsUnavailable.GetCode(), Errors.MerchantSecretsVaultIsUnavailable.GetMessage());
        }
    }

    private string ContextOf(Guid tenant)
    {
        var found = options.Value.Transit.SecretContexts.TryGetValue(SecretContextName, out var context);

        InfrastructureGuard.IsFalse(found && !string.IsNullOrWhiteSpace(context), Errors.MerchantSecretContextIsNotConfigured);

        return $"{context}:{tenant}";
    }
}
