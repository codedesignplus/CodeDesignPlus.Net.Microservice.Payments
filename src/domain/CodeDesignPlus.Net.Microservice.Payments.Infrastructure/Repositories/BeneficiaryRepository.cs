namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class BeneficiaryRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<BeneficiaryRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), IBeneficiaryRepository
{
    public async Task<BeneficiaryAggregate?> GetByUserIdAsync(Guid userId, Guid tenant, CancellationToken cancellationToken)
    {
        var collection = GetCollection<BeneficiaryAggregate>();

        var filter = Builders<BeneficiaryAggregate>.Filter.And(
            Builders<BeneficiaryAggregate>.Filter.Eq(x => x.UserId, userId),
            Builders<BeneficiaryAggregate>.Filter.Eq(x => x.Tenant, tenant),
            Builders<BeneficiaryAggregate>.Filter.Eq(x => x.IsActive, true)
        );

        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
