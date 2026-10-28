namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

/// <summary>
/// Payments to third parties.
/// </summary>
public interface IPayoutRepository : IRepositoryBase
{
    /// <summary>
    /// Saves a new payout unless one with its id already exists.
    /// </summary>
    /// <param name="payout">The payout to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>false</c> when a previous request with the same id already saved it.</returns>
    Task<bool> CreateIfAbsentAsync(PayoutAggregate payout, CancellationToken cancellationToken);
}
