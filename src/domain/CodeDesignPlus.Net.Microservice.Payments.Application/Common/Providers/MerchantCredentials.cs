namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;

/// <summary>
/// The secrets of a merchant account, as they are kept in the Vault.
/// </summary>
/// <remarks>
/// <see cref="ToString"/> is overridden on purpose: a record prints every member, and a secret that reaches a log
/// line ends up in the observability backend, which is exactly where it must never be.
/// </remarks>
/// <param name="PrivateKey">Key that authenticates server-to-server calls to the provider.</param>
/// <param name="EventsSecret">Secret the provider signs its events with.</param>
/// <param name="IntegritySecret">Secret that signs the hosted page URL.</param>
/// <param name="PayoutsPrincipalId">User principal of the payouts API, when payouts are connected.</param>
/// <param name="PayoutsApiKey">Key of the payouts API, when payouts are connected.</param>
public sealed record MerchantSecrets(string PrivateKey, string EventsSecret, string IntegritySecret, string? PayoutsPrincipalId, string? PayoutsApiKey)
{
    /// <summary>
    /// Whether the secrets include the payouts credentials.
    /// </summary>
    public bool HasPayouts => !string.IsNullOrWhiteSpace(PayoutsPrincipalId) && !string.IsNullOrWhiteSpace(PayoutsApiKey);

    /// <summary>
    /// Never prints a secret.
    /// </summary>
    /// <returns>A redacted description.</returns>
    public override string ToString() => $"{nameof(MerchantSecrets)} {{ redacted, payouts: {HasPayouts} }}";

    /// <summary>
    /// The last four characters of a secret, which is all the platform ever shows of it.
    /// </summary>
    /// <param name="secret">The secret.</param>
    /// <returns>Its last four characters, or the whole value when it is shorter.</returns>
    public static string HintOf(string secret) => secret.Length <= 4 ? secret : secret[^4..];
}

/// <summary>
/// Everything a provider adapter needs to act on behalf of a merchant account.
/// </summary>
/// <param name="Provider">Code of the provider of the account.</param>
/// <param name="Environment">Provider environment the keys belong to.</param>
/// <param name="PublicKey">The merchant's public key.</param>
/// <param name="Secrets">The merchant's secrets, read from the Vault.</param>
/// <param name="PayoutsSourceAccountId">Provider account the payouts are paid from, when payouts are connected.</param>
public sealed record MerchantCredentials(string Provider, MerchantAccountEnvironment Environment, string PublicKey, MerchantSecrets Secrets, string? PayoutsSourceAccountId)
{
    /// <summary>
    /// Rejections of these keys the account has counted since they last worked; 0 for the platform's account.
    /// </summary>
    public int PendingRejections { get; init; }

    /// <summary>
    /// Never prints a secret.
    /// </summary>
    /// <returns>A redacted description.</returns>
    public override string ToString() => $"{nameof(MerchantCredentials)} {{ Provider = {Provider}, Environment = {Environment} }}";
}
