
namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class BankRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<BankRepository> logger)

    : RepositoryBase(serviceProvider, mongoOptions, logger), IBankRepository
{
    public async Task<List<BanksAggregate>> GetAllAsync(CancellationToken cancellationToken)
    {
        var collection = base.GetCollection<BanksAggregate>();

        var filter = Builders<BanksAggregate>.Filter.Empty;

        return await collection.Find(filter).ToListAsync(cancellationToken);
    }

}