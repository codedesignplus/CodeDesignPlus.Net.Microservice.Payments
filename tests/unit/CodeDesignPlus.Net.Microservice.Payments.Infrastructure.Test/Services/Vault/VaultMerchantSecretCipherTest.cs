using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Vault;
using CodeDesignPlus.Net.Vault.Abstractions;
using CodeDesignPlus.Net.Vault.Abstractions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Vault;

/// <summary>
/// The secrets of an account are encrypted bound to its tenant: the Transit context carries the tenant id.
/// </summary>
public class VaultMerchantSecretCipherTest
{
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");

    private readonly Mock<IVaultTransit> transit = new();

    [Fact]
    public async Task SealAsync_Secrets_AreEncryptedWithTheTenantInTheContext()
    {
        // Arrange
        transit.Setup(x => x.EncryptAsync(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(("transit-key", new List<string> { "c1", "c2", "c3" }));

        // Act
        await Cipher().SealAsync(Tenant, new MerchantSecrets("prv", "evt", "int", null, null), CancellationToken.None);

        // Assert
        transit.Verify(x => x.EncryptAsync(It.Is<List<string>>(plain => plain.SequenceEqual(new[] { "prv", "evt", "int" })), $"payments-merchant:{Tenant}"), Times.Once);
    }

    [Fact]
    public async Task SealAsync_WithPayouts_KeepsEveryCiphertextAndTheKeyName()
    {
        // Arrange
        transit.Setup(x => x.EncryptAsync(It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(("transit-key", new List<string> { "c1", "c2", "c3", "c4", "c5" }));

        // Act
        var sealedSecrets = await Cipher().SealAsync(Tenant, new MerchantSecrets("prv", "evt", "int", "principal", "api"), CancellationToken.None);

        // Assert
        Assert.Equal(new SealedSecrets("transit-key", "c1", "c2", "c3", "c4", "c5"), sealedSecrets);
    }

    [Fact]
    public async Task OpenAsync_Ciphertext_IsDecryptedWithTheSameTenantContext()
    {
        // Arrange
        transit.Setup(x => x.DecryptAsync("transit-key", It.IsAny<List<string>>(), $"payments-merchant:{Tenant}")).ReturnsAsync(["prv", "evt", "int"]);

        // Act
        var secrets = await Cipher().OpenAsync(Tenant, new SealedSecrets("transit-key", "c1", "c2", "c3", null, null), CancellationToken.None);

        // Assert
        Assert.Equal(("prv", "evt", "int", false), (secrets.PrivateKey, secrets.EventsSecret, secrets.IntegritySecret, secrets.HasPayouts));
    }

    [Fact]
    public async Task SealAsync_ContextNotConfigured_Throws()
    {
        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Cipher(contexts: []).SealAsync(Tenant, new MerchantSecrets("prv", "evt", "int", null, null), CancellationToken.None));

        // Assert
        Assert.Equal(Errors.MerchantSecretContextIsNotConfigured.Code, exception.Code);
    }

    [Fact]
    public async Task OpenAsync_VaultDoesNotAnswer_ThrowsTheRetryableError()
    {
        // Arrange
        transit.Setup(x => x.DecryptAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ThrowsAsync(new HttpRequestException("connection refused"));

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Cipher().OpenAsync(Tenant, new SealedSecrets("transit-key", "c1", "c2", "c3", null, null), CancellationToken.None));

        // Assert
        Assert.Equal((Errors.MerchantSecretsVaultIsUnavailable.Code, true), (exception.Code, CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.TransientFailures.IsTransient(exception)));
    }

    private VaultMerchantSecretCipher Cipher(Dictionary<string, string>? contexts = null)
    {
        var options = new VaultOptions();
        options.Transit.SecretContexts = contexts ?? new Dictionary<string, string> { [VaultMerchantSecretCipher.SecretContextName] = "payments-merchant" };

        return new VaultMerchantSecretCipher(transit.Object, Options.Create(options), Mock.Of<ILogger<VaultMerchantSecretCipher>>());
    }
}
