using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using AppErrors = CodeDesignPlus.Net.Microservice.Payments.Application.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.MerchantAccount;

/// <summary>
/// Keys are validated before anything is stored, and only the last four characters of each secret stay outside the
/// Vault.
/// </summary>
public class MerchantAccountConnectorTest
{
    private readonly Mock<IPaymentProviderFactory> providers = new();
    private readonly Mock<IMerchantCredentialsProvider> validator = new();
    private readonly Mock<IMerchantSecretStore> secretStore = new();

    public MerchantAccountConnectorTest()
    {
        providers.Setup(x => x.GetCredentials("Wompi")).Returns(validator.Object);
        secretStore.Setup(x => x.SaveAsync(It.IsAny<Guid>(), It.IsAny<MerchantSecrets>(), It.IsAny<CancellationToken>())).ReturnsAsync("ms-payments/merchant-accounts/x");
        Validates(true, MerchantAccountEnvironment.Sandbox);
    }

    [Fact]
    public async Task ValidateAndStoreAsync_ValidKeys_KeepsOnlyTheLastFourOfEachSecret()
    {
        // Act
        var connection = await Connector().ValidateAndStoreAsync(TestData.PendingAccount(), Keys(), CancellationToken.None);

        // Assert
        Assert.Equal(("pub_test_tenant", "1111", "2222", "3333", "ms-payments/merchant-accounts/x"), (connection.PublicKey, connection.PrivateKeyHint, connection.EventsSecretHint, connection.IntegritySecretHint, connection.SecretReference));
    }

    [Fact]
    public async Task ValidateAndStoreAsync_RejectedKeys_StoresNothing()
    {
        // Arrange
        Validates(false, MerchantAccountEnvironment.Sandbox);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Connector().ValidateAndStoreAsync(TestData.PendingAccount(), Keys(), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.MerchantKeysWereRejected.GetCode(), exception.Code);
        secretStore.Verify(x => x.SaveAsync(It.IsAny<Guid>(), It.IsAny<MerchantSecrets>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAndStoreAsync_LiveKeysInATestDeployment_StoresNothing()
    {
        // Arrange
        Validates(true, MerchantAccountEnvironment.Production);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Connector().ValidateAndStoreAsync(TestData.PendingAccount(), Keys(), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.MerchantKeysEnvironmentMismatch.GetCode(), exception.Code);
        secretStore.Verify(x => x.SaveAsync(It.IsAny<Guid>(), It.IsAny<MerchantSecrets>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAndStoreAsync_HalfThePayoutsCredentials_ThrowsBeforeValidating()
    {
        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Connector().ValidateAndStoreAsync(TestData.PendingAccount(), Keys() with { PayoutsApiKey = "api-key" }, CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.PayoutsCredentialsAreIncomplete.GetCode(), exception.Code);
        validator.Verify(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<MerchantSecrets>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAndStoreAsync_AccountWithoutProvider_ThrowsNoPaymentProviderForCountry()
    {
        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Connector().ValidateAndStoreAsync(TestData.PendingAccount(provider: null), Keys(), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.NoPaymentProviderForCountry.GetCode(), exception.Code);
    }

    [Fact]
    public void ToString_Keys_PrintsNoSecret()
    {
        // Act
        var text = Keys().ToString() + new MerchantSecrets("prv_test_1111", "events_2222", "integrity_3333", "principal", "api").ToString();

        // Assert
        Assert.False(text.Contains("prv_test_1111", StringComparison.Ordinal) || text.Contains("events_2222", StringComparison.Ordinal) || text.Contains("integrity_3333", StringComparison.Ordinal) || text.Contains("api", StringComparison.Ordinal), text);
    }

    private void Validates(bool isValid, MerchantAccountEnvironment environment)
    {
        validator.Setup(x => x.ValidateAsync("pub_test_tenant", It.IsAny<MerchantSecrets>(), It.IsAny<CancellationToken>())).ReturnsAsync(new MerchantValidation(isValid, environment, "Comercio SAS", isValid ? null : "rejected"));
    }

    private MerchantAccountConnector Connector()
    {
        var options = Options.Create(new PaymentProvidersOptions { Environment = MerchantAccountEnvironment.Sandbox, DefaultReturnUrl = new Uri("https://www.example.com/return"), EventsUrl = new Uri("https://www.example.com/notify/wompi") });

        return new MerchantAccountConnector(providers.Object, secretStore.Object, options, Mock.Of<ILogger<MerchantAccountConnector>>());
    }

    private static MerchantKeys Keys() => new("pub_test_tenant", "prv_test_1111", "events_2222", "integrity_3333", null, null, null);
}
