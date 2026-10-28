namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

/// <summary>
/// What a merchant account keeps after its keys were validated against the provider. Never carries a secret.
/// </summary>
/// <param name="Environment">Provider environment the keys belong to.</param>
/// <param name="PublicKey">The merchant's public key; it is public by design and goes into the hosted page URL.</param>
/// <param name="MerchantName">Name the provider has for the merchant, shown so the tenant recognizes its account.</param>
/// <param name="SecretReference">Where the secrets live in the Vault.</param>
/// <param name="PrivateKeyHint">Last four characters of the private key.</param>
/// <param name="EventsSecretHint">Last four characters of the events secret.</param>
/// <param name="IntegritySecretHint">Last four characters of the integrity secret.</param>
/// <param name="PayoutsSourceAccountId">Provider account the payouts are paid from, when payouts are connected.</param>
/// <param name="PayoutsApiKeyHint">Last four characters of the payouts API key, when payouts are connected.</param>
public sealed record MerchantConnection(
    MerchantAccountEnvironment Environment,
    string PublicKey,
    string? MerchantName,
    string SecretReference,
    string PrivateKeyHint,
    string EventsSecretHint,
    string IntegritySecretHint,
    string? PayoutsSourceAccountId,
    string? PayoutsApiKeyHint);
