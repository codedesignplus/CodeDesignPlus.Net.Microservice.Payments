namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Commands.DeactivateBeneficiary;

public record DeactivateBeneficiaryCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<DeactivateBeneficiaryCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
