namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Commands.CompleteDisbursement;

public record CompleteDisbursementCommand(Guid Id, string ProviderReference) : IRequest;

public class Validator : AbstractValidator<CompleteDisbursementCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProviderReference).NotEmpty();
    }
}
