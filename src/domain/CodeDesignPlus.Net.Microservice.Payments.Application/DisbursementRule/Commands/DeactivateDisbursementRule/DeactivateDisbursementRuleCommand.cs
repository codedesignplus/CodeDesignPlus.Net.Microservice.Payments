namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.DeactivateDisbursementRule;

public record DeactivateDisbursementRuleCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<DeactivateDisbursementRuleCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
