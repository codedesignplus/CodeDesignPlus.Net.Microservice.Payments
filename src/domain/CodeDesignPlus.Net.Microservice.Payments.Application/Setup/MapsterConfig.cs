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
                Status = src.Status,
                Module = src.Module,
                Tenant = src.Tenant,
                Provider = src.Provider,
                PlatformAccount = src.PlatformAccount,
                ProviderTransactionId = src.ProviderTransactionId,
                CapturedAmount = src.CapturedAmount,
                PaymentMethodType = src.PaymentMethodType,
                InitiateResponse = src.InitiateResponse,
                FinalResponse = src.FinalResponse
            });

        // Field by field on purpose: a mapping by convention would copy whatever matches by name, and the
        // aggregate keeps the buyer's document and the provider's raw answers. Listing what goes out is what
        // keeps a new member of the aggregate from being published by itself.
        TypeAdapterConfig<PaymentAggregate, PaymentSummaryDto>
            .NewConfig()
            .MapWith(src => new PaymentSummaryDto
            {
                Id = src.Id,
                Module = src.Module,
                ReferenceId = src.ReferenceId,
                Status = src.Status,
                Total = src.Total,
                Description = src.Description,
                PaymentProvider = src.Provider,
                CreatedAt = src.CreatedAt,
                PaymentMethodType = src.PaymentMethodType,
                BuyerName = src.Buyer.Name
            });

        TypeAdapterConfig<SavedCardAggregate, SavedCardDto>
            .NewConfig()
            .MapWith(src => new SavedCardDto
            {
                Id = src.Id,
                MaskedNumber = src.MaskedNumber,
                Franchise = src.Franchise,
                CardHolderName = src.CardHolderName,
                ExpirationDate = src.ExpirationDate,
                Last4Digits = src.Last4Digits,
                IsDefault = src.IsDefault,
                IsActive = src.IsActive
            });
    }
}
