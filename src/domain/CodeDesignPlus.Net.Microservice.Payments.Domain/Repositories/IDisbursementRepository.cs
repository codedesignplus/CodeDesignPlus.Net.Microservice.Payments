namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

public interface IDisbursementRepository : IRepositoryBase
{
    Task<List<DisbursementAggregate>> GetByPaymentIdAsync(Guid paymentId, Guid tenant, CancellationToken cancellationToken);
}
