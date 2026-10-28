using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Domain;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Test;

/// <summary>
/// The collection account of a tenant: one per tenant, connected once, and never holding a secret.
/// </summary>
public class MerchantAccountAggregateTest
{
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    private static readonly Guid User = Guid.Parse("cc000000-0000-4000-8000-000000000003");

    [Fact]
    public void IdFor_SameTenant_IsAlwaysTheSame()
    {
        // Act
        var ids = (MerchantAccountAggregate.CreatePending(Tenant, "CO", "Wompi", User).Id, MerchantAccountAggregate.CreatePending(Tenant, "CO", "Wompi", User).Id);

        // Assert
        Assert.Equal(ids.Item1, ids.Item2);
    }

    [Fact]
    public void IdFor_Tenant_IsNotTheTenantId()
    {
        // Act
        var id = MerchantAccountAggregate.IdFor(Tenant);

        // Assert
        Assert.NotEqual(Tenant, id);
    }

    [Fact]
    public void Connect_PendingAccount_IsActive()
    {
        // Arrange
        var account = MerchantAccountAggregate.CreatePending(Tenant, "CO", "Wompi", User);

        // Act
        account.Connect(Connection(), User);

        // Assert
        Assert.Equal((MerchantAccountStatus.Active, MerchantAccountEnvironment.Sandbox, "1111"), (account.Status, account.Environment, account.PrivateKeyHint));
    }

    [Fact]
    public void Connect_ActiveAccount_Throws()
    {
        // Arrange
        var account = MerchantAccountAggregate.CreatePending(Tenant, "CO", "Wompi", User);
        account.Connect(Connection(), User);

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => account.Connect(Connection(), User));

        // Assert
        Assert.Equal(Errors.MerchantAccountIsAlreadyConnected.Code, exception.Code);
    }

    [Fact]
    public void Connect_AccountWithoutProvider_Throws()
    {
        // Arrange: no provider serves the tenant's country.
        var account = MerchantAccountAggregate.CreatePending(Tenant, "PE", null, User);

        // Act
        var exception = Assert.Throws<CodeDesignPlusException>(() => account.Connect(Connection(), User));

        // Assert
        Assert.Equal(Errors.PaymentProviderIsRequired.Code, exception.Code);
    }

    [Fact]
    public void Disconnect_ActiveAccount_ForgetsEveryKey()
    {
        // Arrange
        var account = MerchantAccountAggregate.CreatePending(Tenant, "CO", "Wompi", User);
        account.Connect(Connection(payoutsSourceAccountId: "wompi-account-1"), User);

        // Act
        account.Disconnect(User);

        // Assert
        Assert.Equal((MerchantAccountStatus.Disabled, (SealedSecrets?)null, (string?)null, (string?)null, false), (account.Status, account.Secrets, account.PublicKey, account.PayoutsApiKeyHint, account.PayoutsEnabled));
    }

    [Fact]
    public void PayoutsEnabled_ActiveAccountWithSourceAccount_IsTrue()
    {
        // Arrange
        var account = MerchantAccountAggregate.CreatePending(Tenant, "CO", "Wompi", User);

        // Act
        account.Connect(Connection(payoutsSourceAccountId: "wompi-account-1"), User);

        // Assert
        Assert.True(account.PayoutsEnabled);
    }

    private static MerchantConnection Connection(string? payoutsSourceAccountId = null) => new(MerchantAccountEnvironment.Sandbox, "pub_test_tenant", "Comercio SAS", new SealedSecrets("vault:v1:prv", "vault:v1:evt", "vault:v1:int", null, null), "1111", "2222", "3333", payoutsSourceAccountId, payoutsSourceAccountId is null ? null : "4444");
}
