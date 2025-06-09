using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;
using NodaTime;
using NodaTime.Serialization.Protobuf;

namespace CodeDesignPlus.Net.Microservice.Payments.gRpc.Core.Mapster;

public static class MapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<PayRequest, PayCommand>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Transaction, src => src.Transaction);
    }
}