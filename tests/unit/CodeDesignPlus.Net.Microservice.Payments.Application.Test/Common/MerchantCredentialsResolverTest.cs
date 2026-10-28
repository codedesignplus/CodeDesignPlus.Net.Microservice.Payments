using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using Xunit;
using AppErrors = CodeDesignPlus.Net.Microservice.Payments.Application.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;

/// <summary>
/// A tenant charges online only with its own connected account; the platform charges with its configured one.
/// </summary>
public class MerchantCredentialsResolverTest
{
    private readonly Mock<IMerchantAccountRepository> repository = new();
    private readonly Mock<IMerchantSecretCipher> cipher = new();
    private readonly Mock<IMerchantSecretsCache> cache = new();
    private readonly Mock<IPlatformMerchantAccount> platform = new();

    [Fact]
    public async Task ForNewPaymentAsync_TenantWithoutAccount_ThrowsMerchantAccountIsNotActive()
    {
        // Arrange
        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync((MerchantAccountAggregate?)null);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Resolver().ForNewPaymentAsync(TestData.Tenant, false, CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.MerchantAccountIsNotActive.GetCode(), exception.Code);
    }

    [Fact]
    public async Task ForNewPaymentAsync_AccountPendingConnection_ThrowsMerchantAccountIsNotActive()
    {
        // Arrange
        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.PendingAccount());

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Resolver().ForNewPaymentAsync(TestData.Tenant, false, CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.MerchantAccountIsNotActive.GetCode(), exception.Code);
    }

    [Fact]
    public async Task ForNewPaymentAsync_ActiveAccount_DecryptsItsSecretsBoundToItsTenant()
    {
        // Arrange
        var account = TestData.ActiveAccount();
        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        cipher.Setup(x => x.OpenAsync(TestData.Tenant, account.Secrets!, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Secrets());

        // Act
        var credentials = await Resolver().ForNewPaymentAsync(TestData.Tenant, false, CancellationToken.None);

        // Assert
        Assert.Equal(("Wompi", "pub_test_tenant", "prv_test_tenant"), (credentials.Provider, credentials.PublicKey, credentials.Secrets.PrivateKey));
    }

    [Fact]
    public async Task ForNewPaymentAsync_PlatformAccount_IgnoresTheTenantAccount()
    {
        // Arrange
        platform.Setup(x => x.Get()).Returns(TestData.Credentials());

        // Act
        await Resolver().ForNewPaymentAsync(TestData.Tenant, true, CancellationToken.None);

        // Assert
        repository.Verify(x => x.GetByTenantAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForNewPaymentAsync_PlatformAccountNotConfigured_ThrowsPlatformAccountIsNotConfigured()
    {
        // Arrange
        platform.Setup(x => x.Get()).Returns((MerchantCredentials?)null);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Resolver().ForNewPaymentAsync(Guid.Empty, true, CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.PlatformAccountIsNotConfigured.GetCode(), exception.Code);
    }

    [Fact]
    public async Task ForPaymentAsync_PlatformPayment_UsesThePlatformAccount()
    {
        // Arrange
        var expected = TestData.Credentials();
        platform.Setup(x => x.Get()).Returns(expected);

        // Act
        var credentials = await Resolver().ForPaymentAsync(TestData.Payment(platformAccount: true), CancellationToken.None);

        // Assert
        Assert.Same(expected, credentials);
    }

    [Fact]
    public async Task ForPaymentAsync_DisconnectedAccount_HasNoCredentials()
    {
        // Arrange
        var account = TestData.ActiveAccount();
        account.Disconnect(TestData.User);
        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        // Act
        var credentials = await Resolver().ForPaymentAsync(TestData.Payment(), CancellationToken.None);

        // Assert
        Assert.Null(credentials);
    }

    [Fact]
    public async Task ForNewPaymentAsync_SecretsCachedForThisVersion_DoesNotCallTheVault()
    {
        // Arrange
        var account = TestData.ActiveAccount();
        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        cache.Setup(x => x.Get(account.Id, account.Version)).Returns(TestData.Secrets());

        // Act
        await Resolver().ForNewPaymentAsync(TestData.Tenant, false, CancellationToken.None);

        // Assert
        cipher.Verify(x => x.OpenAsync(It.IsAny<Guid>(), It.IsAny<CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects.SealedSecrets>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForNewPaymentAsync_SecretsNotCached_DecryptsOnceAndCachesThem()
    {
        // Arrange
        var account = TestData.ActiveAccount();
        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        cipher.Setup(x => x.OpenAsync(TestData.Tenant, account.Secrets!, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Secrets());

        // Act
        await Resolver().ForNewPaymentAsync(TestData.Tenant, false, CancellationToken.None);

        // Assert
        cache.Verify(x => x.Set(account.Id, account.Version, It.IsAny<MerchantSecrets>()), Times.Once);
    }

    private MerchantCredentialsResolver Resolver() => new(repository.Object, cipher.Object, cache.Object, platform.Object);
}
