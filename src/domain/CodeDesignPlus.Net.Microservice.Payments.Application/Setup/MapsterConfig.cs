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
                InitiateResponse = src.InitiateResponse,
                FinalResponse = src.FinalResponse
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
