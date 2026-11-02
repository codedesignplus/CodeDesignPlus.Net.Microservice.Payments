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
    /// Document types the hosted page accepts to prefill the payer's id. A buyer's document of another type is not
    /// prefilled: the payer types it on the page.
    /// </summary>
    public List<string> LegalIdTypes { get; set; } = [];

    /// <summary>
    /// The platform's own merchant account, which collects the subscriptions sold on the landing.
    /// </summary>
    public WompiPlatformAccountOptions Platform { get; set; } = new();

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
