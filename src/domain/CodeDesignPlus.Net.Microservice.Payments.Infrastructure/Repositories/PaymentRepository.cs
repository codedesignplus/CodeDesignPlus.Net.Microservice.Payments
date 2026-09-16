namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class PaymentRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<PaymentRepository> logger)
    : RepositoryBase(serviceProvider, mongoOptions, logger), IPaymentRepository
{
    /// <inheritdoc/>
    public async Task<List<PaymentAggregate>> GetInProgressOlderThanAsync(Instant cutoff, int limit, CancellationToken cancellationToken)
    {
        var collection = GetCollection<PaymentAggregate>();

        // `In` sobre los dos estados en curso, y no dos `Ne` sobre los resueltos: con `Ne` cualquier estado
        // nuevo que se anadiera al enum entraria en el barrido por omision, y lo que hay que enumerar es lo
        // que se cierra, no lo que se salva.
        var inProgress = new[] { PaymentStatus.Initiated, PaymentStatus.Pending };

        var filter = Builders<PaymentAggregate>.Filter.In(x => x.Status, inProgress)
            & Builders<PaymentAggregate>.Filter.Lt(x => x.CreatedAt, cutoff)
            & Builders<PaymentAggregate>.Filter.Eq(x => x.IsActive, true);

        // Del mas viejo primero: si el barrido se corta por el limite, la siguiente pasada sigue por donde
        // iba en vez de volver a mirar los mismos.
        return await collection
            .Find(filter)
            .SortBy(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }
}
