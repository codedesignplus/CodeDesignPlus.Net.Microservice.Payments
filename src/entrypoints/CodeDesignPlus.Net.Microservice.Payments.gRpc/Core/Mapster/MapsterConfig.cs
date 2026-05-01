using CodeDesignPlus.Net.Exceptions.Extensions;
using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Application;
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

        TypeAdapterConfig<Address, ValueObjects.User.Address>
          .NewConfig()
          .MapWith(src => ValueObjects.User.Address.Create(
              src.Street,
              src.Country,
              src.State,
              src.City,
              src.PostalCode
          ));

        TypeAdapterConfig<Buyer, ValueObjects.User.Buyer>
            .NewConfig()
            .MapWith(src => ValueObjects.User.Buyer.Create(
                Guid.Parse(src.BuyerId),
                src.Name,
                src.Phone,
                src.Email,
                ValueObjects.User.TypeDocument.Create(src.TypeDocument.Code, src.TypeDocument.Name),
                src.Document,
                src.ShippingAddress.Adapt<ValueObjects.User.Address>()
            ));

        TypeAdapterConfig<Payer, ValueObjects.User.Payer>
            .NewConfig()
            .MapWith(src => ValueObjects.User.Payer.Create(
                src.FullName,
                src.EmailAddress,
                src.ContactPhone,
                ValueObjects.User.TypeDocument.Create(src.TypeDocument.Code, src.TypeDocument.Name),
                src.DocumentNumber,
                src.BillingAddress.Adapt<ValueObjects.User.Address>()
            ));

        TypeAdapterConfig<CreditCard, ValueObjects.Payment.CreditCard>
            .NewConfig()
            .MapWith(src => ValueObjects.Payment.CreditCard.Create(
                src.CreditCardTokenId,
                src.Last4Digits,
                src.ExpirationDate,
                src.CardHolderName,
                src.SecurityCode,
                src.InstallmentsNumber
            ));

        TypeAdapterConfig<Pse, ValueObjects.Payment.Pse>
            .NewConfig()
            .MapWith(src => ValueObjects.Payment.Pse.Create(
                src.PseCode,
                src.TypePerson,
                src.PseResponseUrl
            ));

        TypeAdapterConfig<PaymentMethod, ValueObjects.Payment.PaymentMethod>
            .NewConfig()
            .MapWith(src => ValueObjects.Payment.PaymentMethod.Create(
                src.Type,
                src.Pse.Adapt<ValueObjects.Payment.Pse>(),
                src.CreditCard.Adapt<ValueObjects.Payment.CreditCard>()
            ));

        TypeAdapterConfig<InitiatePaymentRequest, InitiatePaymentCommand>
            .NewConfig()
            .MapWith(src => new InitiatePaymentCommand(
                Guid.Parse(src.Id),
                src.Module,
                Guid.Parse(src.ReferenceId),
                src.SubTotal.Adapt<Domain.ValueObjects.Amount>(),
                src.Tax.Adapt<Domain.ValueObjects.Amount>(),
                src.Total.Adapt<Domain.ValueObjects.Amount>(),
                src.Description,
                src.Buyer.Adapt<ValueObjects.User.Buyer>(),
                src.Payer.Adapt<ValueObjects.User.Payer>(),
                src.PaymentMethod.Adapt<ValueObjects.Payment.PaymentMethod>(),
                (Domain.Enums.PaymentProvider)src.Provider
            ));

        TypeAdapterConfig<InitiatePaymentResponseDto, InitiatePaymentResponse>
            .NewConfig()
            .MapWith(src => new InitiatePaymentResponse
            {
                NextAction =(NextActionType)src.NextAction,
                PaymentId = src.PaymentId.ToString(),
                RedirectUrl = src.RedirectUrl,
                Success = src.Success
            });
    }
}