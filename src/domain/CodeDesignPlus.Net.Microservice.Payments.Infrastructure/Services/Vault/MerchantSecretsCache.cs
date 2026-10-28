using System.Collections.Concurrent;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using Microsoft.Extensions.Caching.Memory;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Vault;

/// <summary>
/// The decrypted secrets of the merchant accounts, in this process's memory for
/// <see cref="PaymentProvidersOptions.SecretsCacheLifetime"/>.
/// </summary>
public sealed class MerchantSecretsCache(IMemoryCache cache, IOptions<PaymentProvidersOptions> options) : IMerchantSecretsCache
{
    private readonly ConcurrentDictionary<Guid, long> versions = new();

    /// <inheritdoc/>
    public MerchantSecrets? Get(Guid accountId, long version)
    {
        return cache.TryGetValue(KeyOf(accountId, version), out MerchantSecrets? secrets) ? secrets : null;
    }

    /// <inheritdoc/>
    public void Set(Guid accountId, long version, MerchantSecrets secrets)
    {
        ArgumentNullException.ThrowIfNull(secrets);

        if (versions.TryGetValue(accountId, out var previous) && previous != version)
        {
            cache.Remove(KeyOf(accountId, previous));
        }

        versions[accountId] = version;

        cache.Set(KeyOf(accountId, version), secrets, options.Value.SecretsCacheLifetime);
    }

    /// <inheritdoc/>
    public void Forget(Guid accountId)
    {
        if (versions.TryRemove(accountId, out var version))
        {
            cache.Remove(KeyOf(accountId, version));
        }
    }

    private static string KeyOf(Guid accountId, long version) => $"merchant-secrets:{accountId}:{version}";
}
