namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;

/// <summary>
/// The platform's own Wompi merchant, read from <c>Wompi:Platform</c>. It collects the subscriptions sold on the
/// landing, never a tenant's money.
/// </summary>
public sealed class WompiPlatformAccount(IOptions<WompiOptions> options) : IPlatformMerchantAccount
{
    /// <inheritdoc/>
    public MerchantCredentials? Get()
    {
        var platform = options.Value.Platform;

        if (string.IsNullOrWhiteSpace(platform.PublicKey) || string.IsNullOrWhiteSpace(platform.PrivateKey) || string.IsNullOrWhiteSpace(platform.EventsSecret) || string.IsNullOrWhiteSpace(platform.IntegritySecret))
        {
            return null;
        }

        var environment = WompiKeys.EnvironmentOf(platform.PublicKey);

        if (environment is null)
        {
            return null;
        }

        var secrets = new MerchantSecrets(platform.PrivateKey, platform.EventsSecret, platform.IntegritySecret, null, null);

        return new MerchantCredentials(WompiProvider.ProviderCode, environment.Value, platform.PublicKey, secrets, null);
    }
}
