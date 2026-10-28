namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class MerchantAccountRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<MerchantAccountRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), IMerchantAccountRepository
{
    /// <inheritdoc/>
    public async Task<MerchantAccountAggregate?> GetByTenantAsync(Guid tenant, CancellationToken cancellationToken)
    {
        var filter = Builders<MerchantAccountAggregate>.Filter.Eq(x => x.Id, MerchantAccountAggregate.IdFor(tenant));

        return await GetCollection<MerchantAccountAggregate>().Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
