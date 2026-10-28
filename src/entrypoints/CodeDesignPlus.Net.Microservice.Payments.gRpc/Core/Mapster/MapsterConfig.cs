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

        // The page prefill does not need a shipping address, which the contract no longer carries.
        TypeAdapterConfig<Buyer, ValueObjects.User.Buyer>
            .NewConfig()
            .MapWith(src => src.TypeDocument != null && src.HasDocument
                ? ValueObjects.User.Buyer.CreateWithoutShipping(
                    Guid.Parse(src.BuyerId),
                    src.Name,
                    src.Phone,
                    src.Email,
                    ValueObjects.User.TypeDocument.Create(src.TypeDocument.Code, src.TypeDocument.Name),
                    src.Document)
                : ValueObjects.User.Buyer.CreateMinimal(
                    Guid.Parse(src.BuyerId),
                    src.Name,
                    src.Phone,
                    src.Email));

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
                src.HasProvider && !string.IsNullOrWhiteSpace(src.Provider) ? src.Provider : null,
                src.HasReturnUrl && !string.IsNullOrWhiteSpace(src.ReturnUrl) ? src.ReturnUrl : null,
                src.PlatformAccount
            ));

        TypeAdapterConfig<InitiatePaymentResponseDto, InitiatePaymentResponse>
            .NewConfig()
            .MapWith(src => new InitiatePaymentResponse
            {
                NextAction = (NextActionType)src.NextAction,
                PaymentId = src.PaymentId.ToString(),
                RedirectUrl = src.RedirectUrl,
                Success = src.Success
            });
    }
}
