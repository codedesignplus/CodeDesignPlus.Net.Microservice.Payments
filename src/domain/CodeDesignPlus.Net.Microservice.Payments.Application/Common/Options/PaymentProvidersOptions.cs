using System.ComponentModel.DataAnnotations;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;

/// <summary>
/// How this deployment chooses and uses the payment providers.
/// </summary>
public class PaymentProvidersOptions : IValidatableObject
{
    /// <summary>
    /// Name of the section in the configuration.
    /// </summary>
    public static readonly string Section = "PaymentProviders";

    /// <summary>
    /// Provider code by ISO 3166-1 alpha-2 country (for example <c>CO = Wompi</c>). A country without an entry has
    /// no online payments.
    /// </summary>
    public Dictionary<string, string> Routing { get; set; } = [];

    /// <summary>
    /// Provider environment this deployment accepts keys for: a tenant cannot connect test keys where real money
    /// moves, nor live keys in a test deployment.
    /// </summary>
    public MerchantAccountEnvironment Environment { get; set; } = MerchantAccountEnvironment.Sandbox;

    /// <summary>
    /// Page the provider returns to when the caller of a payment does not send one.
    /// </summary>
    [Required]
    public Uri DefaultReturnUrl { get; set; } = null!;

    /// <summary>
    /// The webhook URL every merchant must configure on its provider panel. Shown to the tenant when it connects.
    /// </summary>
    [Required]
    public Uri EventsUrl { get; set; } = null!;

    /// <summary>
    /// How long the provider's hosted page accepts a payment after it is created.
    /// </summary>
    public TimeSpan CheckoutLifetime { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// How old a payment in progress must be before the reconciliation asks the provider about it, so the payer still
    /// on the hosted page is not polled.
    /// </summary>
    public TimeSpan ReconciliationMinimumAge { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Transit key that encrypts the secrets of every tenant's collection account. It is derived by context: the
    /// tenant in the context keeps one tenant's ciphertext from decrypting as another's.
    /// </summary>
    [Required]
    public string SecretsKeyName { get; set; } = null!;

    /// <summary>
    /// How long the decrypted secrets of an account stay in this process's memory. Short on purpose: it only absorbs
    /// the peaks, and a rotated key in another process is noticed through the version of the account anyway.
    /// </summary>
    public TimeSpan SecretsCacheLifetime { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How many rejections of the keys in a row invalidate a collection account. More than one, so a transient failure
    /// never stops a tenant from charging online.
    /// </summary>
    public int InvalidateAfterRejections { get; set; } = 3;

    /// <summary>
    /// How many payments the reconciliation asks about in a run.
    /// </summary>
    public int ReconciliationBatchSize { get; set; } = 200;

    /// <summary>
    /// The rates each provider charges by default, by provider code. A tenant that has not set its own rates on its
    /// collection account pays these; the platform's account always does.
    /// </summary>
    public Dictionary<string, ProviderFeeScheduleOptions> DefaultFees { get; set; } = [];

    /// <summary>
    /// The default rates of a provider.
    /// </summary>
    /// <param name="provider">The provider code.</param>
    /// <returns>The rates, or null when the deployment has none for that provider.</returns>
    public ProviderFeeSchedule? DefaultFeeFor(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return null;
        }

        var entry = DefaultFees.FirstOrDefault(x => string.Equals(x.Key, provider, StringComparison.OrdinalIgnoreCase));

        return entry.Value?.ToSchedule();
    }

    /// <summary>
    /// The provider routed for a country.
    /// </summary>
    /// <param name="country">ISO 3166-1 alpha-2 code.</param>
    /// <returns>The provider code, or null when no provider serves the country.</returns>
    public string? ProviderFor(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return null;
        }

        var route = Routing.FirstOrDefault(x => string.Equals(x.Key, country, StringComparison.OrdinalIgnoreCase));

        return string.IsNullOrWhiteSpace(route.Value) ? null : route.Value;
    }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Environment))
        {
            yield return new ValidationResult("Environment must be Sandbox or Production.", [nameof(Environment)]);
        }

        if (CheckoutLifetime <= TimeSpan.Zero)
        {
            yield return new ValidationResult("CheckoutLifetime must be greater than zero.", [nameof(CheckoutLifetime)]);
        }

        if (ReconciliationMinimumAge <= TimeSpan.Zero)
        {
            yield return new ValidationResult("ReconciliationMinimumAge must be greater than zero.", [nameof(ReconciliationMinimumAge)]);
        }

        if (SecretsCacheLifetime <= TimeSpan.Zero)
        {
            yield return new ValidationResult("SecretsCacheLifetime must be greater than zero.", [nameof(SecretsCacheLifetime)]);
        }

        if (InvalidateAfterRejections <= 0)
        {
            yield return new ValidationResult("InvalidateAfterRejections must be greater than zero.", [nameof(InvalidateAfterRejections)]);
        }

        if (ReconciliationBatchSize <= 0)
        {
            yield return new ValidationResult("ReconciliationBatchSize must be greater than zero.", [nameof(ReconciliationBatchSize)]);
        }

        foreach (var (provider, fee) in DefaultFees)
        {
            foreach (var problem in fee?.Problems() ?? ["the rates are missing"])
            {
                yield return new ValidationResult($"DefaultFees:{provider}: {problem}.", [nameof(DefaultFees)]);
            }
        }
    }
}

/// <summary>
/// The default rates of a provider, as they are written in the configuration.
/// </summary>
public class ProviderFeeScheduleOptions
{
    /// <summary>
    /// The rate of every payment method without its own.
    /// </summary>
    public ProviderFeeRateOptions Default { get; set; } = null!;

    /// <summary>
    /// The payment methods with a rate of their own, by the code the provider gives the method (for example <c>CARD</c>).
    /// </summary>
    public Dictionary<string, ProviderFeeRateOptions> ByPaymentMethod { get; set; } = [];

    /// <summary>
    /// The rates as the domain keeps them.
    /// </summary>
    /// <returns>The schedule.</returns>
    public ProviderFeeSchedule ToSchedule() => ProviderFeeSchedule.Create(Default.ToRate(), ByPaymentMethod.Select(x => new PaymentMethodFeeRate(x.Key, x.Value.ToRate())));

    /// <summary>
    /// What is wrong with the rates, so a bad configuration stops the start-up instead of computing wrong commissions.
    /// </summary>
    /// <returns>One text per problem; none when the rates are fine.</returns>
    public IEnumerable<string> Problems()
    {
        if (Default is null)
        {
            yield return "the Default rate is missing";
            yield break;
        }

        foreach (var problem in Default.Problems())
        {
            yield return $"Default {problem}";
        }

        foreach (var (method, rate) in ByPaymentMethod)
        {
            foreach (var problem in rate?.Problems() ?? ["is missing"])
            {
                yield return $"ByPaymentMethod:{method} {problem}";
            }
        }
    }
}

/// <summary>
/// A rate of a provider, as it is written in the configuration: basis points and minor units (rules 04 and 05).
/// </summary>
public class ProviderFeeRateOptions
{
    /// <summary>
    /// Percentage of the gross, in basis points (265 = 2.65 %).
    /// </summary>
    public int RateBasisPoints { get; set; }

    /// <summary>
    /// Fixed amount per payment, in minor units of <see cref="Currency"/>.
    /// </summary>
    public long FixedAmount { get; set; }

    /// <summary>
    /// ISO 4217 currency of <see cref="FixedAmount"/>.
    /// </summary>
    public string Currency { get; set; } = null!;

    /// <summary>
    /// Tax on the commission, in basis points (1900 = 19 %).
    /// </summary>
    public int TaxRateBasisPoints { get; set; }

    /// <summary>
    /// The rate as the domain keeps it.
    /// </summary>
    /// <returns>The rate.</returns>
    public ProviderFeeRate ToRate() => ProviderFeeRate.Create(RateBasisPoints, Money.FromLong(FixedAmount, Currency), TaxRateBasisPoints);

    /// <summary>
    /// What is wrong with the rate.
    /// </summary>
    /// <returns>One text per problem.</returns>
    public IEnumerable<string> Problems()
    {
        if (RateBasisPoints < 0 || FixedAmount < 0 || TaxRateBasisPoints < 0)
        {
            yield return "cannot be negative";
        }

        if (string.IsNullOrWhiteSpace(Currency) || Currency.Length != 3)
        {
            yield return "needs the ISO 4217 Currency of its FixedAmount";
        }
    }
}
