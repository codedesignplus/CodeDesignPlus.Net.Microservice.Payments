namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class DisbursementRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<DisbursementRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), IDisbursementRepository
{
    public async Task<List<DisbursementAggregate>> GetByPaymentIdAsync(Guid paymentId, Guid tenant, CancellationToken cancellationToken)
    {
        var collection = GetCollection<DisbursementAggregate>();

        var filter = Builders<DisbursementAggregate>.Filter.And(
            Builders<DisbursementAggregate>.Filter.Eq(x => x.PaymentId, paymentId),
            Builders<DisbursementAggregate>.Filter.Eq(x => x.Tenant, tenant)
        );

        return await collection.Find(filter).ToListAsync(cancellationToken);
    }
}
