namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.CreateSavedCard;

[DtoGenerator]
public record CreateSavedCardCommand(
    Guid Id,
    string Token,
    string MaskedNumber,
    string Franchise,
    string CardHolderName,
    string ExpirationDate,
    string Last4Digits
) : IRequest;

public class Validator : AbstractValidator<CreateSavedCardCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Token).NotEmpty().MaximumLength(255);
        RuleFor(x => x.MaskedNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Franchise).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CardHolderName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ExpirationDate)
            .NotEmpty()
            .Length(7)
            .Matches(@"^\d{4}/\d{2}$")
            .WithMessage("Expiration date must be in YYYY/MM format.");
        RuleFor(x => x.Last4Digits)
            .NotEmpty()
            .Length(4)
            .Matches(@"^\d{4}$")
            .WithMessage("Last 4 digits must be exactly 4 numeric characters.");
    }
}
