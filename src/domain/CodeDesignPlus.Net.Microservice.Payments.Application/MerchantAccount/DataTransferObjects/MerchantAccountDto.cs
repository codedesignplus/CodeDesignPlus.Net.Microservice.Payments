using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;

/// <summary>
/// The collection account of a tenant as the screen and the other services see it. Never carries a secret.
/// </summary>
/// <remarks>
/// Of each secret only its last four characters travel, so the tenant can tell which keys are connected; a null hint
/// means the secret is not configured.
/// </remarks>
public class MerchantAccountDto
{
    /// <summary>
    /// Id of the account, derived from the tenant.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant that owns the account.
    /// </summary>
    public Guid Tenant { get; set; }

    /// <summary>
    /// Provider routed for the tenant's country, or null when none serves it.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Current state of the connection.
    /// </summary>
    public MerchantAccountStatus Status { get; set; }

    /// <summary>
    /// Provider environment of the connected keys.
    /// </summary>
    public MerchantAccountEnvironment? Environment { get; set; }

    /// <summary>
    /// The merchant's public key.
    /// </summary>
    public string? PublicKey { get; set; }

    /// <summary>
    /// Name the provider has for the merchant.
    /// </summary>
    public string? MerchantName { get; set; }

    /// <summary>
    /// Last four characters of the private key.
    /// </summary>
    public string? PrivateKeyHint { get; set; }

    /// <summary>
    /// Last four characters of the events secret.
    /// </summary>
    public string? EventsSecretHint { get; set; }

    /// <summary>
    /// Last four characters of the integrity secret.
    /// </summary>
    public string? IntegritySecretHint { get; set; }

    /// <summary>
    /// Whether the account can pay third parties.
    /// </summary>
    public bool PayoutsEnabled { get; set; }

    /// <summary>
    /// Provider account the payouts are paid from.
    /// </summary>
    public string? PayoutsSourceAccountId { get; set; }

    /// <summary>
    /// Last four characters of the payouts API key.
    /// </summary>
    public string? PayoutsApiKeyHint { get; set; }

    /// <summary>
    /// When the keys were last validated against the provider.
    /// </summary>
    public Instant? LastValidatedAt { get; set; }

    /// <summary>
    /// Code of the error catalog that says why the provider stopped accepting the keys, while the account is invalid.
    /// </summary>
    public string? InvalidReasonCode { get; set; }

    /// <summary>
    /// The same reason, in the language of the request.
    /// </summary>
    public string? InvalidReasonMessage { get; set; }

    /// <summary>
    /// The webhook URL the tenant must configure on its provider panel; the same for every tenant.
    /// </summary>
    public string? EventsUrl { get; set; }

    /// <summary>
    /// Builds the view of an account.
    /// </summary>
    /// <param name="account">The account.</param>
    /// <param name="eventsUrl">The webhook URL of the deployment.</param>
    /// <returns>The view, without secrets.</returns>
    public static MerchantAccountDto From(MerchantAccountAggregate account, Uri eventsUrl) => new()
    {
        Id = account.Id,
        Tenant = account.Tenant,
        Provider = account.Provider,
        Status = account.Status,
        Environment = account.Environment,
        PublicKey = account.PublicKey,
        MerchantName = account.MerchantName,
        PrivateKeyHint = account.PrivateKeyHint,
        EventsSecretHint = account.EventsSecretHint,
        IntegritySecretHint = account.IntegritySecretHint,
        PayoutsEnabled = account.PayoutsEnabled,
        PayoutsSourceAccountId = account.PayoutsSourceAccountId,
        PayoutsApiKeyHint = account.PayoutsApiKeyHint,
        LastValidatedAt = account.LastValidatedAt,
        InvalidReasonCode = account.InvalidReasonCode,
        InvalidReasonMessage = account.InvalidReasonCode is null ? null : new Error(account.InvalidReasonCode).GetMessage(),
        EventsUrl = eventsUrl.ToString()
    };

    /// <summary>
    /// The view of a tenant that has no account yet: it is waiting for its keys.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="provider">Provider routed for the tenant's country, when known.</param>
    /// <param name="eventsUrl">The webhook URL of the deployment.</param>
    /// <returns>A pending account.</returns>
    public static MerchantAccountDto Pending(Guid tenant, string? provider, Uri eventsUrl) => new()
    {
        Id = MerchantAccountAggregate.IdFor(tenant),
        Tenant = tenant,
        Provider = provider,
        Status = MerchantAccountStatus.PendingConnection,
        EventsUrl = eventsUrl.ToString()
    };
}
