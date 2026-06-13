using CodeDesignPlus.Net.Microservice.Payments.Application.Common;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.TokenizeCard;

public class TokenizeCardCommandHandler(IPaymentProviderAdapterFactory adapterFactory, ISavedCardRepository repository, IUserContext userContext, IMapper mapper)
    : IRequestHandler<TokenizeCardCommand, SavedCardDto>
{
    public async Task<SavedCardDto> Handle(TokenizeCardCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var existingCard = await repository.GetCreditCardsAsync(userContext.IdUser, request.CardNumber, request.ExpirationDate, cancellationToken);

        if (existingCard != null)
            return mapper.Map<SavedCardDto>(existingCard);

        var adapter = adapterFactory.GetAdapter(request.PaymentProvider);

        ApplicationGuard.IsNull(adapter, Errors.PaymentProviderNotSupported);

        var tokenResponse = await adapter.TokenizeCreditCardAsync(
            request.Name,
            request.IdentificationNumber,
            request.PaymentMethod,
            request.CardNumber,
            request.ExpirationDate,
            cancellationToken
        );

        ApplicationGuard.IsNull(tokenResponse, Errors.TokenizationFailed);

        var savedCardDto = mapper.Map<SavedCardDto>(tokenResponse);

        savedCardDto.CardHolderName = request.Name;
        savedCardDto.Franchise = request.PaymentMethod;
        savedCardDto.ExpirationDate = request.ExpirationDate;
        savedCardDto.Last4Digits = request.CardNumber[^4..];

        return savedCardDto;
    }
}
