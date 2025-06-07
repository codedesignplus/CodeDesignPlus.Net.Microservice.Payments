namespace CodeDesignPlus.Net.Microservice.Payments.Application.Banks.Commands.SyncBanks;

[DtoGenerator]
public record SyncBanksCommand(IEnumerable<BanksDto> Banks) : IRequest;

public class Validator : AbstractValidator<SyncBanksCommand>
{
    public Validator()
    {
        RuleFor(x => x.Banks)
            .NotEmpty()
            .NotNull()
            .WithMessage("Banks list cannot be null or empty.")
            .Must(banks => banks.All(bank => bank != null))
            .WithMessage("All banks in the list must be non-null.");

        RuleForEach(x => x.Banks)
            .SetValidator(new BankValidator());
    }
}

public class BankValidator : AbstractValidator<BanksDto>
{
    public BankValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .NotNull()
            .MaximumLength(255)
            .WithMessage("Description cannot be null or empty and must not exceed 255 characters.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .NotNull()
            .MaximumLength(50)
            .WithMessage("Code cannot be null or empty and must not exceed 50 characters.");
    }
}