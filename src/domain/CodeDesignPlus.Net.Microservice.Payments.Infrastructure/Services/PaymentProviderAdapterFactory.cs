using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;

public class PaymentProviderAdapterFactory(IServiceProvider serviceProvider) : IPaymentProviderAdapterFactory
{
    public IPaymentProviderAdapter GetAdapter(PaymentProvider provider)
    {
        return serviceProvider.GetRequiredKeyedService<IPaymentProviderAdapter>(provider);
    }
}
