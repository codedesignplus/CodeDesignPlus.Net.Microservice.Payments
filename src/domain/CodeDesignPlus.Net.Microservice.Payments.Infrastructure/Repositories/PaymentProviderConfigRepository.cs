namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class PaymentProviderConfigRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<PaymentProviderConfigRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), IPaymentProviderConfigRepository
{
    public async Task<PaymentProviderConfigAggregate?> GetByProviderAndTenantAsync(PaymentProvider provider, Guid tenant, CancellationToken cancellationToken)
    {
        var collection = GetCollection<PaymentProviderConfigAggregate>();
        var filter = Builders<PaymentProviderConfigAggregate>.Filter.And(
            Builders<PaymentProviderConfigAggregate>.Filter.Eq(x => x.Provider, provider),
            Builders<PaymentProviderConfigAggregate>.Filter.Eq(x => x.Tenant, tenant),
            Builders<PaymentProviderConfigAggregate>.Filter.Eq(x => x.IsActive, true),
            Builders<PaymentProviderConfigAggregate>.Filter.Eq(x => x.IsDeleted, false));

        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaymentProviderConfigAggregate?> GetByMerchantIdAsync(string merchantId, CancellationToken cancellationToken)
    {
        var collection = GetCollection<PaymentProviderConfigAggregate>();
        var filter = Builders<PaymentProviderConfigAggregate>.Filter.And(
            Builders<PaymentProviderConfigAggregate>.Filter.Eq(x => x.MerchantId, merchantId),
            Builders<PaymentProviderConfigAggregate>.Filter.Eq(x => x.IsActive, true),
            Builders<PaymentProviderConfigAggregate>.Filter.Eq(x => x.IsDeleted, false));

        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
