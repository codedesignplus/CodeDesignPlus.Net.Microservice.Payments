namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

public interface ISavedCardRepository : IRepositoryBase
{
    Task<List<SavedCardAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
