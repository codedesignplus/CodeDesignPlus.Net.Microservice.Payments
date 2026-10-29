using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

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
    /// The rates the provider charges the tenant: its own if it set them, the provider's default otherwise. Rates in
    /// basis points and the fixed amount in minor units (rules 04 and 05).
    /// </summary>
    public ProviderFeeScheduleDto? FeeSchedule { get; set; }

    /// <summary>
    /// Whether <see cref="FeeSchedule"/> is the provider's default because the tenant has not set its own.
    /// </summary>
    public bool FeeScheduleIsDefault { get; set; }

    /// <summary>
    /// Builds the view of an account.
    /// </summary>
    /// <param name="account">The account.</param>
    /// <param name="options">The deployment's providers: the webhook URL and the default rates.</param>
    /// <returns>The view, without secrets.</returns>
    public static MerchantAccountDto From(MerchantAccountAggregate account, PaymentProvidersOptions options) => new()
    {
        FeeSchedule = ProviderFeeScheduleDto.From(account.FeeSchedule ?? options.DefaultFeeFor(account.Provider)),
        FeeScheduleIsDefault = account.FeeSchedule is null,
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
        EventsUrl = options.EventsUrl.ToString()
    };

    /// <summary>
    /// The view of a tenant that has no account yet: it is waiting for its keys.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="provider">Provider routed for the tenant's country, when known.</param>
    /// <param name="options">The deployment's providers: the webhook URL and the default rates.</param>
    /// <returns>A pending account.</returns>
    public static MerchantAccountDto Pending(Guid tenant, string? provider, PaymentProvidersOptions options) => new()
    {
        FeeSchedule = ProviderFeeScheduleDto.From(options.DefaultFeeFor(provider)),
        FeeScheduleIsDefault = true,
        Id = MerchantAccountAggregate.IdFor(tenant),
        Tenant = tenant,
        Provider = provider,
        Status = MerchantAccountStatus.PendingConnection,
        EventsUrl = options.EventsUrl.ToString()
    };
}

/// <summary>
/// The rates a provider charges, as the screen reads them.
/// </summary>
public class ProviderFeeScheduleDto
{
    /// <summary>
    /// The rate of every payment method without its own.
    /// </summary>
    public ProviderFeeRateDto Default { get; set; } = null!;

    /// <summary>
    /// The payment methods with a rate of their own.
    /// </summary>
    public List<PaymentMethodFeeRateDto> ByPaymentMethod { get; set; } = [];

    /// <summary>
    /// The view of a schedule.
    /// </summary>
    /// <param name="schedule">The schedule, or null.</param>
    /// <returns>The view, or null when there is no schedule.</returns>
    public static ProviderFeeScheduleDto? From(ProviderFeeSchedule? schedule) => schedule is null ? null : new()
    {
        Default = ProviderFeeRateDto.From(schedule.Default),
        ByPaymentMethod = [.. (schedule.ByPaymentMethod ?? []).Select(x => new PaymentMethodFeeRateDto { PaymentMethodType = x.PaymentMethodType, Rate = ProviderFeeRateDto.From(x.Rate) })]
    };
}

/// <summary>
/// One rate of a provider.
/// </summary>
public class ProviderFeeRateDto
{
    /// <summary>
    /// Percentage of the gross, in basis points.
    /// </summary>
    public int RateBasisPoints { get; set; }

    /// <summary>
    /// Fixed amount per payment, in minor units of <see cref="Currency"/>.
    /// </summary>
    public long FixedAmount { get; set; }

    /// <summary>
    /// ISO 4217 currency of the fixed amount.
    /// </summary>
    public string Currency { get; set; } = null!;

    /// <summary>
    /// Tax on the commission, in basis points.
    /// </summary>
    public int TaxRateBasisPoints { get; set; }

    /// <summary>
    /// The view of a rate.
    /// </summary>
    /// <param name="rate">The rate.</param>
    /// <returns>The view.</returns>
    public static ProviderFeeRateDto From(ProviderFeeRate rate) => new()
    {
        RateBasisPoints = rate.RateBasisPoints,
        FixedAmount = rate.FixedAmount.Amount,
        Currency = rate.FixedAmount.Currency,
        TaxRateBasisPoints = rate.TaxRateBasisPoints
    };
}

/// <summary>
/// The rate of one payment method.
/// </summary>
public class PaymentMethodFeeRateDto
{
    /// <summary>
    /// Payment method as the provider names it.
    /// </summary>
    public string PaymentMethodType { get; set; } = null!;

    /// <summary>
    /// Its rate.
    /// </summary>
    public ProviderFeeRateDto Rate { get; set; } = null!;
}
