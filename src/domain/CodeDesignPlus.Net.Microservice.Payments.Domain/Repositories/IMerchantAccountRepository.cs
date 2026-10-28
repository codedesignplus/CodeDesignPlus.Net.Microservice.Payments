namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

/// <summary>
/// Collection accounts of the tenants.
/// </summary>
public interface IMerchantAccountRepository : IRepositoryBase
{
    /// <summary>
    /// The account of a tenant, or null when it has none yet.
    /// </summary>
    /// <param name="tenant">The tenant that owns the account.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant's account, or null.</returns>
    Task<MerchantAccountAggregate?> GetByTenantAsync(Guid tenant, CancellationToken cancellationToken);
}
