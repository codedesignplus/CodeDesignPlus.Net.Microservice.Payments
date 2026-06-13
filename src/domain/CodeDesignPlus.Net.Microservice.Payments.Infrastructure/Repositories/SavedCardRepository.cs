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

    public async Task<SavedCardAggregate> GetCreditCardsAsync(Guid userId, string cardNumber, string expirationDate, CancellationToken cancellationToken)
    {
        var bin = cardNumber[..6];
        var last4 = cardNumber[^4..];

        var collection = GetCollection<SavedCardAggregate>();

        var filter = Builders<SavedCardAggregate>.Filter.And(
            Builders<SavedCardAggregate>.Filter.Eq(x => x.UserId, userId),
            Builders<SavedCardAggregate>.Filter.Regex(x => x.MaskedNumber, new MongoDB.Bson.BsonRegularExpression($"^{bin}.*{last4}$")),
            Builders<SavedCardAggregate>.Filter.Eq(x => x.ExpirationDate, expirationDate),
            Builders<SavedCardAggregate>.Filter.Eq(x => x.IsActive, true)
        );

        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

}
