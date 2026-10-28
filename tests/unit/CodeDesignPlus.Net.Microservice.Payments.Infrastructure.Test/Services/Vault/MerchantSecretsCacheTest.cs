using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Vault;
using Microsoft.Extensions.Caching.Memory;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Vault;

/// <summary>
/// Decrypted secrets stay in memory per account and version, and are dropped when the keys change.
/// </summary>
public class MerchantSecretsCacheTest
{
    private static readonly Guid Account = Guid.NewGuid();
    private static readonly MerchantSecrets Secrets = new("prv", "evt", "int", null, null);

    private readonly MerchantSecretsCache cache = new(new MemoryCache(new MemoryCacheOptions()), Options.Create(new PaymentProvidersOptions()));

    [Fact]
    public void Get_SameVersion_ReturnsTheCachedSecrets()
    {
        // Arrange
        cache.Set(Account, 3, Secrets);

        // Act
        var cached = cache.Get(Account, 3);

        // Assert
        Assert.Same(Secrets, cached);
    }

    [Fact]
    public void Get_NewerVersion_MissesSoTheNewKeysAreDecrypted()
    {
        // Arrange: another process replaced the keys and the account moved to version 4.
        cache.Set(Account, 3, Secrets);

        // Act
        var cached = cache.Get(Account, 4);

        // Assert
        Assert.Null(cached);
    }

    [Fact]
    public void Set_NewerVersion_DropsTheEarlierOne()
    {
        // Arrange
        cache.Set(Account, 3, Secrets);

        // Act
        cache.Set(Account, 4, Secrets with { PrivateKey = "prv2" });

        // Assert
        Assert.Null(cache.Get(Account, 3));
    }

    [Fact]
    public void Forget_Account_DropsItsSecrets()
    {
        // Arrange
        cache.Set(Account, 3, Secrets);

        // Act
        cache.Forget(Account);

        // Assert
        Assert.Null(cache.Get(Account, 3));
    }
}
