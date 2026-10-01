using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common;

public interface IPaymentProviderAdapterFactory
{
    IPaymentProviderAdapter GetAdapter(PaymentProvider provider);
}
