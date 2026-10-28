using CodeDesignPlus.Net.Microservice.Payments.Domain;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Helpers;
using CodeDesignPlus.Net.Mongo.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Repositories;

/// <summary>
/// The collection account against a real Mongo: one per tenant however many deliveries create it at once, and it
/// reads back as it was saved.
/// </summary>
[Collection(MongoContainerFixture.Collection)]
public class MerchantAccountRepositoryTest
{
    private readonly IMerchantAccountRepository repository;

    public MerchantAccountRepositoryTest(MongoContainerFixture fixture)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mongo:Enable"] = "true",
                ["Mongo:ConnectionString"] = fixture.ConnectionString,
                ["Mongo:Database"] = $"db-ms-payments-{Guid.NewGuid():N}",
                ["Mongo:RegisterHealthCheck"] = "false",
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMongo<Startup>(configuration);

        this.repository = services.BuildServiceProvider().GetRequiredService<IMerchantAccountRepository>();
    }

    [Fact]
    public async Task UpdateWithRetryAsync_TwoDeliveriesCreateAtOnce_LeavesASingleAccount()
    {
        // Arrange
        var tenant = Guid.NewGuid();

        Task<bool> Create() => repository.UpdateWithRetryAsync(async () => await repository.GetByTenantAsync(tenant, CancellationToken.None) ?? MerchantAccountAggregate.CreatePending(tenant, "CO", "Wompi", Guid.NewGuid()), account =>
        {
            if (account.Version > 0)
            {
                return false;
            }

            return true;
        }, CancellationToken.None);

        // Act
        var results = await Task.WhenAll(Create(), Create());

        // Assert
        Assert.Single(results, written => written);
    }

    [Fact]
    public async Task GetByTenantAsync_ConnectedAccount_ReadsBackItsConnection()
    {
        // Arrange
        var tenant = Guid.NewGuid();
        var account = MerchantAccountAggregate.CreatePending(tenant, "CO", "Wompi", Guid.NewGuid());
        account.Connect(new MerchantConnection(MerchantAccountEnvironment.Sandbox, "pub_test_tenant", "Comercio SAS", "ms-payments/merchant-accounts/x", "1111", "2222", "3333", "wompi-account-1", "4444"), Guid.NewGuid());

        await repository.UpdateWithRetryAsync(() => Task.FromResult(account), _ => true, CancellationToken.None);

        // Act
        var stored = await repository.GetByTenantAsync(tenant, CancellationToken.None);

        // Assert
        Assert.Equal((MerchantAccountStatus.Active, (MerchantAccountEnvironment?)MerchantAccountEnvironment.Sandbox, "1111", true, 1L), (stored!.Status, stored.Environment, stored.PrivateKeyHint, stored.PayoutsEnabled, stored.Version));
    }
}
