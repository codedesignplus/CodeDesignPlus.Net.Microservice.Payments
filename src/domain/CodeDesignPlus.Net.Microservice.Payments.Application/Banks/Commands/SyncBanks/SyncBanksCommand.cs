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
            .WithErrorCode(Errors.BanksListIsRequired.Code)
            .Must(banks => banks.All(bank => bank != null))
            .WithErrorCode(Errors.BanksListHasEmptyEntries.Code);

        RuleForEach(x => x.Banks)
            .SetValidator(new BankValidator());
    }
}

public class BankValidator : AbstractValidator<BanksDto>
{
    public BankValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .NotNull()
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .NotEmpty()
            .NotNull()
            .MaximumLength(255);

        RuleFor(x => x.Code)
            .NotEmpty()
            .NotNull()
            .MaximumLength(50);
    }
}