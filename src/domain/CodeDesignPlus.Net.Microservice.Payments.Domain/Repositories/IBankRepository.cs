namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

public interface IBankRepository : IRepositoryBase
{
    Task<List<BanksAggregate>> GetAllAsync(CancellationToken cancellationToken);
}