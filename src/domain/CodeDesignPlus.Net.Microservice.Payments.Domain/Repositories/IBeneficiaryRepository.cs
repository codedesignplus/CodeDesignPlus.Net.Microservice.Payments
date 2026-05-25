namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

public interface IBeneficiaryRepository : IRepositoryBase
{
    Task<BeneficiaryAggregate?> GetByUserIdAsync(Guid userId, Guid tenant, CancellationToken cancellationToken);
}
