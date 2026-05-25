namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

public interface IDisbursementRuleRepository : IRepositoryBase
{
    Task<DisbursementRuleAggregate?> GetActiveByTenantAsync(Guid tenant, CancellationToken cancellationToken);
}
