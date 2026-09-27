using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Domain;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Helpers;
using CodeDesignPlus.Net.Mongo.Abstractions.Options;
using CodeDesignPlus.Net.Mongo.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Repositories;

/// <summary>
/// Purgar los pagos de una copropiedad contra un Mongo real: se borran los suyos y se conserva la venta de licencias.
/// </summary>
/// <remarks>
/// Los pagos se insertan como documentos crudos con solo los campos que mira el filtro: armar un
/// <see cref="PaymentAggregate"/> completo no añade nada a lo que se prueba, que es el filtro del borrado.
/// </remarks>
[Collection(MongoContainerFixture.Collection)]
public class PaymentRepositoryTest
{
    private readonly IPaymentRepository repository;
    private readonly IMongoCollection<BsonDocument> payments;

    public PaymentRepositoryTest(MongoContainerFixture fixture)
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

        var provider = services.BuildServiceProvider();

        this.repository = provider.GetRequiredService<IPaymentRepository>();

        var database = provider.GetRequiredService<IOptions<MongoOptions>>().Value.Database;

        this.payments = provider.GetRequiredService<IMongoClient>().GetDatabase(database).GetCollection<BsonDocument>(nameof(PaymentAggregate));
    }

    private async Task<Guid> InsertAsync(Guid? tenant, string module)
    {
        var id = Guid.NewGuid();

        await payments.InsertOneAsync(new BsonDocument
        {
            ["_id"] = new BsonBinaryData(id, GuidRepresentation.Standard),
            ["Module"] = module,
            ["Tenant"] = tenant.HasValue ? new BsonBinaryData(tenant.Value, GuidRepresentation.Standard) : BsonNull.Value,
        });

        return id;
    }

    private Task<long> CountAsync(Guid id) => payments.CountDocumentsAsync(Builders<BsonDocument>.Filter.Eq("_id", new BsonBinaryData(id, GuidRepresentation.Standard)));

    [Fact]
    public async Task DeleteByTenantKeepingLicensesAsync_MixedPayments_DeletesOnlyTheTenantPaymentsThatAreNotLicenses()
    {
        // Arrange
        var purged = Guid.NewGuid();
        var other = Guid.NewGuid();

        var fee = await InsertAsync(purged, "Invoicing");
        var booking = await InsertAsync(purged, "CommonAreas");
        var license = await InsertAsync(purged, PaymentAggregate.LicensesModule);
        var otherTenant = await InsertAsync(other, "Invoicing");
        var withoutTenant = await InsertAsync(null, PaymentAggregate.LicensesModule);

        // Act
        var deleted = await repository.DeleteByTenantKeepingLicensesAsync(purged, CancellationToken.None);

        // Assert
        Assert.Equal(2, deleted);
        Assert.Equal(0, await CountAsync(fee));
        Assert.Equal(0, await CountAsync(booking));
        Assert.Equal(1, await CountAsync(license));
        Assert.Equal(1, await CountAsync(otherTenant));
        Assert.Equal(1, await CountAsync(withoutTenant));
    }

    [Fact]
    public async Task DeleteByTenantKeepingLicensesAsync_EmptyTenant_Throws()
    {
        // Act & Assert
        await Assert.ThrowsAsync<CodeDesignPlusException>(() => repository.DeleteByTenantKeepingLicensesAsync(Guid.Empty, CancellationToken.None));
    }
}
