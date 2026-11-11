using System.ComponentModel.DataAnnotations;
using CodeDesignPlus.Net.Resilience.Abstractions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;

/// <summary>
/// How to reach Wompi, and the platform's own merchant account.
/// </summary>
public class WompiOptions : IValidatableObject
{
    /// <summary>
    /// Name of the section in the configuration.
    /// </summary>
    public static readonly string Section = "Wompi";

    /// <summary>
    /// Collection API for test keys.
    /// </summary>
    [Required]
    public Uri SandboxUrl { get; set; } = null!;

    /// <summary>
    /// Collection API for live keys.
    /// </summary>
    [Required]
    public Uri ProductionUrl { get; set; } = null!;

    /// <summary>
    /// The hosted page (Web Checkout); the same for both environments, the public key tells them apart.
    /// </summary>
    [Required]
    public Uri CheckoutUrl { get; set; } = null!;

    /// <summary>
    /// Payouts API for test keys.
    /// </summary>
    [Required]
    public Uri PayoutsSandboxUrl { get; set; } = null!;

    /// <summary>
    /// Payouts API for live keys.
    /// </summary>
    [Required]
    public Uri PayoutsProductionUrl { get; set; } = null!;

    /// <summary>
    /// The hosted page's document type for each code of the document type catalog, e.g. "13" (DIAN code of the
    /// cedula) to "CC". A buyer's document of a type not listed is not prefilled: the payer types it on the page.
    /// </summary>
    /// <remarks>
    /// The catalog code is the country's (in Colombia, the DIAN's) and Wompi has its own list, so the two are mapped by
    /// configuration (Kappali pendings/663). Before, the catalog code was sent as is, which only worked while the catalog
    /// used Wompi's abbreviations.
    /// </remarks>
    public Dictionary<string, string> LegalIdTypeCodes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The platform's own merchant account, which collects the subscriptions sold on the landing.
    /// </summary>
    public WompiPlatformAccountOptions Platform { get; set; } = new();

    /// <summary>
    /// How long the hosted page lets the payer retry with another payment method after a declined or failed attempt, with the
    /// same reference (3 minutes, docs/wompi/reintento-de-pago.md). Meanwhile the payment waits as AwaitingRetry instead of
    /// closing as failed (pendings/622).
    /// </summary>
    public TimeSpan CheckoutRetryWindow { get; set; } = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Timeouts, retries and circuit breaker of the calls to Wompi.
    /// </summary>
    public ResilienceOptions Resilience { get; set; } = new();

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var platformKeys = new[] { Platform.PublicKey, Platform.PrivateKey, Platform.EventsSecret, Platform.IntegritySecret };

        // All or nothing: a half-configured platform account would charge and then reject its own events.
        if (platformKeys.Any(x => !string.IsNullOrWhiteSpace(x)) && platformKeys.Any(string.IsNullOrWhiteSpace))
        {
            yield return new ValidationResult("Wompi:Platform needs PublicKey, PrivateKey, EventsSecret and IntegritySecret, or none of them.", [nameof(Platform)]);
        }

        if (CheckoutRetryWindow < TimeSpan.Zero)
        {
            yield return new ValidationResult("Wompi:CheckoutRetryWindow cannot be negative.", [nameof(CheckoutRetryWindow)]);
        }

        if (!string.IsNullOrWhiteSpace(Platform.PublicKey) && WompiKeys.EnvironmentOf(Platform.PublicKey) is null)
        {
            yield return new ValidationResult("Wompi:Platform:PublicKey is not a Wompi public key.", [nameof(Platform)]);
        }
    }
}

/// <summary>
/// The keys of the platform's own Wompi merchant. They come from the Vault, never from a file in the repository.
/// </summary>
public class WompiPlatformAccountOptions
{
    /// <summary>
    /// Public key of the platform's merchant.
    /// </summary>
    public string? PublicKey { get; set; }

    /// <summary>
    /// Private key of the platform's merchant.
    /// </summary>
    public string? PrivateKey { get; set; }

    /// <summary>
    /// Events secret of the platform's merchant.
    /// </summary>
    public string? EventsSecret { get; set; }

    /// <summary>
    /// Integrity secret of the platform's merchant.
    /// </summary>
    public string? IntegritySecret { get; set; }

    /// <summary>
    /// Never prints a key.
    /// </summary>
    /// <returns>A redacted description.</returns>
    public override string ToString() => $"{nameof(WompiPlatformAccountOptions)} {{ configured: {!string.IsNullOrWhiteSpace(PublicKey)} }}";
}
