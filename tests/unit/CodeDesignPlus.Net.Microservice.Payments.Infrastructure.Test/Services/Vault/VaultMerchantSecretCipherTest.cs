using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Vault;
using CodeDesignPlus.Net.Vault.Abstractions;
using CodeDesignPlus.Net.Vault.Abstractions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Vault;

/// <summary>
/// Every account is encrypted with the same configured Transit key, bound to its tenant by the context.
/// </summary>
public class VaultMerchantSecretCipherTest
{
    private const string KeyName = "merchant-accounts";

    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    private static readonly string Context = $"payments-merchant:{Tenant}";

    private readonly Mock<IVaultTransit> transit = new();

    [Fact]
    public async Task SealAsync_Secrets_UseTheFixedKeyAndTheTenantContext()
    {
        // Arrange
        transit.Setup(x => x.EncryptAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(["c1", "c2", "c3"]);

        // Act
        await Cipher().SealAsync(Tenant, new MerchantSecrets("prv", "evt", "int", null, null), CancellationToken.None);

        // Assert
        transit.Verify(x => x.EncryptAsync(KeyName, It.Is<List<string>>(plain => plain.SequenceEqual(new[] { "prv", "evt", "int" })), Context), Times.Once);
    }

    [Fact]
    public async Task SealAsync_Secrets_NeverCreateAKeyPerEncryption()
    {
        // Arrange: the overload without a key name creates a new Transit key on every call.
        transit.Setup(x => x.EncryptAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(["c1", "c2", "c3"]);

        // Act
        await Cipher().SealAsync(Tenant, new MerchantSecrets("prv", "evt", "int", null, null), CancellationToken.None);

        // Assert
        transit.Verify(x => x.EncryptAsync(It.IsAny<List<string>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SealAsync_WithPayouts_KeepsEveryCiphertextInOrder()
    {
        // Arrange
        transit.Setup(x => x.EncryptAsync(KeyName, It.IsAny<List<string>>(), Context)).ReturnsAsync(["c1", "c2", "c3", "c4", "c5"]);

        // Act
        var sealedSecrets = await Cipher().SealAsync(Tenant, new MerchantSecrets("prv", "evt", "int", "principal", "api"), CancellationToken.None);

        // Assert
        Assert.Equal(new SealedSecrets("c1", "c2", "c3", "c4", "c5"), sealedSecrets);
    }

    [Fact]
    public async Task OpenAsync_Ciphertext_IsDecryptedInOneBatchWithTheFixedKeyAndTheTenantContext()
    {
        // Arrange
        transit.Setup(x => x.DecryptAsync(KeyName, It.IsAny<List<string>>(), Context)).ReturnsAsync(["prv", "evt", "int"]);

        // Act
        var secrets = await Cipher().OpenAsync(Tenant, new SealedSecrets("c1", "c2", "c3", null, null), CancellationToken.None);

        // Assert
        Assert.Equal(("prv", "evt", "int", false), (secrets.PrivateKey, secrets.EventsSecret, secrets.IntegritySecret, secrets.HasPayouts));
    }

    [Fact]
    public async Task OpenAsync_VaultDoesNotAnswer_ThrowsTheRetryableError()
    {
        // Arrange
        transit.Setup(x => x.DecryptAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ThrowsAsync(new HttpRequestException("connection refused"));

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Cipher().OpenAsync(Tenant, new SealedSecrets("c1", "c2", "c3", null, null), CancellationToken.None));

        // Assert
        Assert.Equal((Errors.MerchantSecretsVaultIsUnavailable.Code, true), (exception.Code, CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.TransientFailures.IsTransient(exception)));
    }

    [Fact]
    public async Task SealAsync_ContextNotConfigured_Throws()
    {
        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Cipher(contexts: []).SealAsync(Tenant, new MerchantSecrets("prv", "evt", "int", null, null), CancellationToken.None));

        // Assert
        Assert.Equal(Errors.MerchantSecretContextIsNotConfigured.Code, exception.Code);
    }

    private VaultMerchantSecretCipher Cipher(Dictionary<string, string>? contexts = null)
    {
        var vault = new VaultOptions();
        vault.Transit.SecretContexts = contexts ?? new Dictionary<string, string> { [VaultMerchantSecretCipher.SecretContextName] = "payments-merchant" };

        var providers = new PaymentProvidersOptions { SecretsKeyName = KeyName };

        return new VaultMerchantSecretCipher(transit.Object, Options.Create(vault), Options.Create(providers), Mock.Of<ILogger<VaultMerchantSecretCipher>>());
    }
}
