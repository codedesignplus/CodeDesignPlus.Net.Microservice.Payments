using System.ComponentModel.DataAnnotations;

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
    public TimeSpan CheckoutLifetime { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// How old a payment in progress must be before the reconciliation asks the provider about it, so the payer still
    /// on the hosted page is not polled.
    /// </summary>
    public TimeSpan ReconciliationMinimumAge { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How long the decrypted secrets of an account stay in this process's memory. Short on purpose: it only absorbs
    /// the peaks, and a rotated key in another process is noticed through the version of the account anyway.
    /// </summary>
    public TimeSpan SecretsCacheLifetime { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// How many payments the reconciliation asks about in a run.
    /// </summary>
    public int ReconciliationBatchSize { get; set; } = 200;

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

        if (ReconciliationBatchSize <= 0)
        {
            yield return new ValidationResult("ReconciliationBatchSize must be greater than zero.", [nameof(ReconciliationBatchSize)]);
        }
    }
}
