using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Setup;

public static class MapsterConfigPayment
{
    public static void Configure()
    {
        TypeAdapterConfig<CodeDesignPlus.Microservice.Api.Dtos.PayDto, PayCommand>
            .NewConfig()
            .ConstructUsing(src => new PayCommand(src.Id, src.Transaction));

        TypeAdapterConfig<PaymentMethodAggregate, PaymentMethodDto>
            .NewConfig();

        TypeAdapterConfig<PaymentAggregate, PaymentDto>
            .NewConfig()
            .MapWith(src => new PaymentDto
            {
                Id = src.Id,
                Provider = src.Provider,
                Request = src.Request,
                Response = src.Response,
                Transaction = src.Transaction
            });

    }
}
