namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

/// <summary>
/// The secrets of a merchant account, encrypted by the Vault's Transit engine. Only ciphertext: the Vault holds the
/// key (the same one for every account, <c>PaymentProviders:SecretsKeyName</c>), and the context it was encrypted with
/// binds it to its tenant. Each ciphertext carries the key version it was encrypted with, so rotating the key keeps it
/// readable.
/// </summary>
/// <param name="PrivateKey">Ciphertext of the private key.</param>
/// <param name="EventsSecret">Ciphertext of the events secret.</param>
/// <param name="IntegritySecret">Ciphertext of the integrity secret.</param>
/// <param name="PayoutsPrincipalId">Ciphertext of the payouts user principal, when payouts are connected.</param>
/// <param name="PayoutsApiKey">Ciphertext of the payouts API key, when payouts are connected.</param>
public sealed record SealedSecrets(string PrivateKey, string EventsSecret, string IntegritySecret, string? PayoutsPrincipalId, string? PayoutsApiKey);
