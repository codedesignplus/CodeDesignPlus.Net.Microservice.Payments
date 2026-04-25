using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.TokenizeCard;

[DtoGenerator]
public record TokenizeCardCommand(
    string Name,
    string IdentificationNumber,
    string PaymentMethod,
    string CardNumber,
    string ExpirationDate,
    PaymentProvider PaymentProvider
) : IRequest<TokenizeCardResponseDto>;

public class TokenizeCardCommandValidator : AbstractValidator<TokenizeCardCommand>
{
    public TokenizeCardCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.IdentificationNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CardNumber).NotEmpty().CreditCard();
        RuleFor(x => x.ExpirationDate)
            .NotEmpty()
            .Length(7)
            .Matches(@"^\d{4}/\d{2}$")
            .WithMessage("Expiration date must be in YYYY/MM format.");
        RuleFor(x => x.PaymentProvider).IsInEnum().NotEqual(PaymentProvider.None);
    }
}
