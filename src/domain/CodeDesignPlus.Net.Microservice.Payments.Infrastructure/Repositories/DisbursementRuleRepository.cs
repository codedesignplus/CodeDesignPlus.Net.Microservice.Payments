namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class DisbursementRuleRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<DisbursementRuleRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), IDisbursementRuleRepository
{
    public async Task<DisbursementRuleAggregate?> GetActiveByTenantAsync(Guid tenant, CancellationToken cancellationToken)
    {
        var collection = GetCollection<DisbursementRuleAggregate>();

        var filter = Builders<DisbursementRuleAggregate>.Filter.And(
            Builders<DisbursementRuleAggregate>.Filter.Eq(x => x.Tenant, tenant),
            Builders<DisbursementRuleAggregate>.Filter.Eq(x => x.IsActive, true)
        );

        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
