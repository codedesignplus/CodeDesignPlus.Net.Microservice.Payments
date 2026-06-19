namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

public interface IPaymentProviderConfigRepository : IRepositoryBase
{
    Task<PaymentProviderConfigAggregate?> GetByProviderAndTenantAsync(PaymentProvider provider, Guid tenant, CancellationToken cancellationToken);
    Task<PaymentProviderConfigAggregate?> GetByMerchantIdAsync(string merchantId, CancellationToken cancellationToken);
}
