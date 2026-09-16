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

        // Campo a campo y a mano, igual que el de arriba, pero aqui la razon no es la comodidad: el mapeo
        // por convencion copiaria todo lo que coincida de nombre, y el agregado guarda el token de la
        // tarjeta, el titular, el codigo de seguridad y el documento del comprador. Enumerar lo que sale es
        // lo que hace que anadir un campo al agregado no lo publique solo.
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
                PaymentProvider = src.PaymentProvider,
                CreatedAt = src.CreatedAt,
                PaymentMethodType = src.PaymentMethod.Type,
                BuyerName = src.Buyer.Name,

                // De la tarjeta, lo unico que se puede mostrar. Es nulo cuando se pago por PSE, que no
                // tiene tarjeta detras.
                Last4Digits = src.PaymentMethod.CreditCard != null ? src.PaymentMethod.CreditCard.Last4Digits : null
            });

        TypeAdapterConfig<SavedCardAggregate, SavedCardDto>
            .NewConfig()
            .MapWith(src => new SavedCardDto
            {
                Id = src.Id,
                Token = src.Token,
                MaskedNumber = src.MaskedNumber,
                Franchise = src.Franchise,
                CardHolderName = src.CardHolderName,
                ExpirationDate = src.ExpirationDate,
                Last4Digits = src.Last4Digits,
                IsDefault = src.IsDefault,
                IsActive = src.IsActive
            });

        TypeAdapterConfig<TokenizeCardResponseDto, SavedCardDto>
            .NewConfig()
            .MapWith(src => new SavedCardDto
            {
                Id = Guid.Empty,
                Token = src.CreditCardTokenId ?? string.Empty,
                MaskedNumber = src.MaskedNumber,
                Franchise = src.PaymentMethod ?? string.Empty,
                CardHolderName = src.Name ?? string.Empty,
                ExpirationDate = string.Empty,
                Last4Digits = string.Empty,
                IsDefault = false,
                IsActive = false
            });


    }
}
