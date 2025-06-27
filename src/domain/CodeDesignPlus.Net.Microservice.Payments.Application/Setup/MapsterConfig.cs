using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Setup;

public static class MapsterConfigPayment
{
    public static void Configure()
    {
        TypeAdapterConfig<PaymentMethodAggregate, PaymentMethodDto>
            .NewConfig();

        TypeAdapterConfig<PaymentAggregate, PaymentDto>
            .NewConfig()
            .MapWith(src => new PaymentDto
            {
                Id = src.Id,
                Description = src.Description,
                SubTotal = src.SubTotal,
                Tax = src.Tax,
                Total = src.Total,
                Payer = src.Payer,
                PaymentMethod = src.PaymentMethod,
                Status = src.Status,
                Module = src.Module,
                Tenant = src.Tenant,
                PaymentProvider = src.PaymentProvider,
                ProviderTransactionId = src.ProviderTransactionId,
                ProviderResponseMessage = src.ProviderResponseMessage,
                RawProviderResponseData = src.RawProviderResponseData,
                FinancialNetwork = src.FinancialNetwork
            });

    }
}
