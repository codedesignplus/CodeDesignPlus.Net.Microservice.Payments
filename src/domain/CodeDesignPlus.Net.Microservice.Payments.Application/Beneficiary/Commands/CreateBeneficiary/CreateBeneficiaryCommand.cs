using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Commands.CreateBeneficiary;

[DtoGenerator]
public record CreateBeneficiaryCommand(
    Guid Id,
    Guid UserId,
    string BankCode,
    string BankName,
    AccountType AccountType,
    string AccountNumber,
    string DocumentType,
    string DocumentNumber,
    string HolderName,
    string Country,
    string Currency,
    string? SwiftBic,
    string? Iban,
    string? RoutingNumber
) : IRequest;

public class Validator : AbstractValidator<CreateBeneficiaryCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BankCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BankName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AccountType).IsInEnum();
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(10);
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.HolderName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).NotEmpty().Length(2).Matches(@"^[A-Z]{2}$");
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches(@"^[A-Z]{3}$");
    }
}
