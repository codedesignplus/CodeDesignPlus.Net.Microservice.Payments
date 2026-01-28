using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;

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

        TypeAdapterConfig<Payer, Domain.ValueObjects.Payer>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.Payer.Create(
                src.FullName,
                src.EmailAddress,
                src.ContactPhone,
                src.DniNumber,
                src.DniType,
                src.BillingAddress.Adapt<Domain.ValueObjects.Address>()
            ));

        TypeAdapterConfig<CreditCard, Domain.ValueObjects.CreditCard>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.CreditCard.Create(
                src.Number,
                src.SecurityCode,
                src.ExpirationDate,
                src.Name,
                (sbyte)src.InstallmentsNumber
            ));

        TypeAdapterConfig<Pse, Domain.ValueObjects.Pse>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.Pse.Create(
                src.PseCode,
                src.TypePerson,
                src.PseResponseUrl
            ));

        TypeAdapterConfig<PaymentMethod, Domain.ValueObjects.PaymentMethod>
            .NewConfig()
            .MapWith(src => Domain.ValueObjects.PaymentMethod.Create(
                src.Type,
                src.CreditCard.Adapt<Domain.ValueObjects.CreditCard>(),
                src.Pse.Adapt<Domain.ValueObjects.Pse>()
            ));

        TypeAdapterConfig<InitiatePaymentRequest, InitiatePaymentCommand>
            .NewConfig()
            .MapWith(src => new InitiatePaymentCommand(
                Guid.Parse(src.Id),
                src.Module,
                src.SubTotal.Adapt<Domain.ValueObjects.Amount>(),
                src.Tax.Adapt<Domain.ValueObjects.Amount>(),
                src.Total.Adapt<Domain.ValueObjects.Amount>(),
                src.Description,
                src.Payer.Adapt<Domain.ValueObjects.Payer>(),
                src.PaymentMethod.Adapt<Domain.ValueObjects.PaymentMethod>(),
                (Domain.Enums.PaymentProvider)src.Provider
            ));
    }
}