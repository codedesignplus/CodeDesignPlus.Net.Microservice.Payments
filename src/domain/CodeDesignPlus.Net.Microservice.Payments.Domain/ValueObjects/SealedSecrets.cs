namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

/// <summary>
/// The secrets of a merchant account, encrypted by the Vault's Transit engine. Only ciphertext: the Vault holds the
/// key, and the context it was encrypted with binds it to its tenant.
/// </summary>
/// <param name="KeyName">Transit key the secrets were encrypted with.</param>
/// <param name="PrivateKey">Ciphertext of the private key.</param>
/// <param name="EventsSecret">Ciphertext of the events secret.</param>
/// <param name="IntegritySecret">Ciphertext of the integrity secret.</param>
/// <param name="PayoutsPrincipalId">Ciphertext of the payouts user principal, when payouts are connected.</param>
/// <param name="PayoutsApiKey">Ciphertext of the payouts API key, when payouts are connected.</param>
public sealed record SealedSecrets(string KeyName, string PrivateKey, string EventsSecret, string IntegritySecret, string? PayoutsPrincipalId, string? PayoutsApiKey);
