using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;

public class PaymentProviderAdapterFactory(IServiceProvider serviceProvider) : IPaymentProviderAdapterFactory
{
    public IPaymentProviderAdapter GetAdapter(PaymentProvider provider)
    {
        throw new NotImplementedException();
    }
}
