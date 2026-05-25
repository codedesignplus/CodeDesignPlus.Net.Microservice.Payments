namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Commands.CreateDisbursement;

[DtoGenerator]
public record CreateDisbursementCommand(
    Guid Id,
    Guid PaymentId,
    Guid BeneficiaryId,
    Guid BeneficiaryUserId,
    long TotalAmount,
    long CommissionAmount,
    long DisbursedAmount,
    string Currency
) : IRequest;

public class Validator : AbstractValidator<CreateDisbursementCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.BeneficiaryId).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
        RuleFor(x => x.DisbursedAmount).GreaterThan(0);
    }
}
