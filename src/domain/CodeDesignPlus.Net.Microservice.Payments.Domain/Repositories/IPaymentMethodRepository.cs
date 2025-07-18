using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

public interface IPaymentMethodRepository : IRepositoryBase
{
    public Task<List<PaymentMethodAggregate>> GetByProviderAsync(PaymentProvider provider, List<TypePaymentMethod> methods, CancellationToken cancellationToken);
}