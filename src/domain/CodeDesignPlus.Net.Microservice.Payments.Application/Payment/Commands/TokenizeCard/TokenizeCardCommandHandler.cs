using CodeDesignPlus.Net.Microservice.Payments.Application.Common;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.TokenizeCard;

public class TokenizeCardCommandHandler(IPaymentProviderAdapterFactory adapterFactory)
    : IRequestHandler<TokenizeCardCommand, TokenizeCardResponseDto>
{
    public async Task<TokenizeCardResponseDto> Handle(TokenizeCardCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var adapter = adapterFactory.GetAdapter(request.PaymentProvider);

        return await adapter.TokenizeCardAsync(
            request.Name,
            request.IdentificationNumber,
            request.PaymentMethod,
            request.CardNumber,
            request.ExpirationDate,
            cancellationToken
        );
    }
}
