using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;

public class PaymentProviderAdapterFactory(IServiceProvider serviceProvider, IPaymentProviderConfigRepository configRepository) : IPaymentProviderAdapterFactory
{
    public IPaymentProviderAdapter GetAdapter(PaymentProvider provider)
    {
        return serviceProvider.GetRequiredKeyedService<IPaymentProviderAdapter>(provider);
    }

    public async Task<AdapterResolutionResult> GetAdapterAsync(PaymentProvider provider, Guid tenantId, CancellationToken cancellationToken)
    {
        var config = await configRepository.GetByProviderAndTenantAsync(provider, tenantId, cancellationToken);
        var adapter = serviceProvider.GetRequiredKeyedService<IPaymentProviderAdapter>(provider);

        return new AdapterResolutionResult
        {
            Adapter = adapter,
            Config = config
        };
    }
}
