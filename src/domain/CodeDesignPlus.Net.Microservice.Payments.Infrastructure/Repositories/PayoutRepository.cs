namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class PayoutRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<PayoutRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), IPayoutRepository
{
    /// <inheritdoc/>
    public async Task<bool> CreateIfAbsentAsync(PayoutAggregate payout, CancellationToken cancellationToken)
    {
        try
        {
            await CreateAsync(payout, cancellationToken);

            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }
}
