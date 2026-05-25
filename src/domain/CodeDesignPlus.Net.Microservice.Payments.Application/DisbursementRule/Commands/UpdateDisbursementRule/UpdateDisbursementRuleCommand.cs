using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.UpdateDisbursementRule;

[DtoGenerator]
public record UpdateDisbursementRuleCommand(
    Guid Id,
    CommissionType CommissionType,
    long CommissionAmount,
    string Currency,
    string? Description
) : IRequest;

public class Validator : AbstractValidator<UpdateDisbursementRuleCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CommissionAmount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
