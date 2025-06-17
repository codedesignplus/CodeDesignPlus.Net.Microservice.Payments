using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;
using NodaTime;
using NodaTime.Serialization.Protobuf;

namespace CodeDesignPlus.Net.Microservice.Payments.gRpc.Core.Mapster;

public static class MapsterConfig
{
    public static void Configure()
    {
         TypeAdapterConfig<Amount, Domain.ValueObjects.Amount>
          .NewConfig()
            .MapWith(src => Domain.ValueObjects.Amount.Create(
                src.Value,
                src.Currency
            ));

        TypeAdapterConfig<Address, Domain.ValueObjects.Address>
          .NewConfig()
          .MapWith(src => Domain.ValueObjects.Address.Create(
              src.Street,
              src.Country,
              src.State,
              src.City,
              src.PostalCode,
              src.Phone
          ));

        TypeAdapterConfig<Buyer, Domain.ValueObjects.Buyer>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.Buyer.Create(
                src.FullName,
                src.EmailAddress,
                src.ContactPhone,
                src.ShippingAddress.Adapt<Domain.ValueObjects.Address>(),
                src.DniNumber,
                src.DniType
            ));

        TypeAdapterConfig<Payer, Domain.ValueObjects.Payer>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.Payer.Create(
                src.EmailAddress,
                src.FullName,
                src.BillingAddress.Adapt<Domain.ValueObjects.Address>(),
                src.DniNumber,
                src.ContactPhone,
                src.DniType
            ));

        TypeAdapterConfig<Order, Domain.ValueObjects.Order>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.Order.Create(
                    src.Description,
                    src.Buyer.Adapt<Domain.ValueObjects.Buyer>(),
                    src.Ammount.Adapt<Domain.ValueObjects.Amount>(),
                    src.Tax.Adapt<Domain.ValueObjects.Amount>(),
                    src.TaxReturnBase.Adapt<Domain.ValueObjects.Amount>()
                )
            );

        TypeAdapterConfig<CreditCard, Domain.ValueObjects.CreditCard>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.CreditCard.Create(
                src.Number,
                src.SecurityCode,
                src.ExpirationDate,
                src.Name
            ));

        TypeAdapterConfig<Pse, Domain.ValueObjects.Pse>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.Pse.Create(
                src.PseCode,
                src.TypePerson,
                src.PseResponseUrl
            ));

        TypeAdapterConfig<Transaction, Domain.ValueObjects.Transaction>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.Transaction.Create(
                src.Order.Adapt<Domain.ValueObjects.Order>(),
                src.Payer.Adapt<Domain.ValueObjects.Payer>(),
                src.CreditCard.Adapt<Domain.ValueObjects.CreditCard?>(),
                src.Pse.Adapt<Domain.ValueObjects.Pse?>(),
                src.DeviceSessionId,
                src.IpAddress,
                src.Cookie,
                src.UserAgent,
                src.PaymentMethod
            ));

        TypeAdapterConfig<PayRequest, PayCommand>
            .NewConfig()
            .MapWith(src => new PayCommand(Guid.Parse(src.Id), src.Module, src.Transaction.Adapt<Domain.ValueObjects.Transaction>()));

        TypeAdapterConfig<CodeDesignPlus.Microservice.Api.Dtos.PayDto, PayCommand>
            .NewConfig()
            .ConstructUsing(src => new PayCommand(src.Id, src.Module, src.Transaction.Adapt<Domain.ValueObjects.Transaction>()));
    }
}