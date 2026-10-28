using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;

/// <summary>
/// Payments, accounts and credentials shared by the tests of the collection flow.
/// </summary>
internal static class TestData
{
    public static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    public static readonly Guid User = Guid.Parse("cc000000-0000-4000-8000-000000000003");

    public static MerchantSecrets Secrets() => new("prv_test_tenant", "test_events_secret", "test_integrity_secret", null, null);

    public static MerchantCredentials Credentials() => new("Wompi", MerchantAccountEnvironment.Sandbox, "pub_test_tenant", Secrets(), null);

    public static PaymentAggregate Payment(Guid? id = null, bool platformAccount = false)
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");
        var buyer = Buyer.CreateWithoutShipping(User, "Ana Restrepo", "3001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        return PaymentAggregate.Create(id ?? Guid.NewGuid(), "Invoicing", Guid.NewGuid(), subTotal, tax, subTotal + tax, buyer, "Cuota de administración", "Wompi", platformAccount, platformAccount ? null : Tenant, User);
    }

    public static MerchantAccountAggregate PendingAccount(string? provider = "Wompi") => MerchantAccountAggregate.CreatePending(Tenant, "CO", provider, User);

    public static MerchantAccountAggregate ActiveAccount(string? payoutsSourceAccountId = null)
    {
        var account = PendingAccount();

        account.Connect(new MerchantConnection(MerchantAccountEnvironment.Sandbox, "pub_test_tenant", "Comercio SAS", new SealedSecrets("vault:v1:prv", "vault:v1:evt", "vault:v1:int", null, null), "nant", "cret", "cret", payoutsSourceAccountId, payoutsSourceAccountId is null ? null : "4321"), User);
        account.GetAndClearEvents();

        return account;
    }
}
