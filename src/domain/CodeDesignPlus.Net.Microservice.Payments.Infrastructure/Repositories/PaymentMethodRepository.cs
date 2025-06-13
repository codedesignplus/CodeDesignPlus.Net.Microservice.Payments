using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class PaymentMethodRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<PaymentMethodRepository> logger)

    : RepositoryBase(serviceProvider, mongoOptions, logger), IPaymentMethodRepository
{
    public Task<List<PaymentMethodAggregate>> GetByProviderAsync(Provider provider, List<TypePaymentMethod> methods, CancellationToken cancellationToken)
    {
        var collection = GetCollection<PaymentMethodAggregate>();

        var filter = Builders<PaymentMethodAggregate>.Filter.And(
            Builders<PaymentMethodAggregate>.Filter.Eq(x => x.Provider, provider),
            Builders<PaymentMethodAggregate>.Filter.In(x => x.Type, methods)
        );

        return collection.Find(filter).ToListAsync(cancellationToken);
    }

}