namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class SavedCardRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<SavedCardRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), ISavedCardRepository
{
    public async Task<List<SavedCardAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var collection = GetCollection<SavedCardAggregate>();

        var filter = Builders<SavedCardAggregate>.Filter.And(
            Builders<SavedCardAggregate>.Filter.Eq(x => x.UserId, userId),
            Builders<SavedCardAggregate>.Filter.Eq(x => x.IsActive, true)
        );

        return await collection.Find(filter).ToListAsync(cancellationToken);
    }
}
