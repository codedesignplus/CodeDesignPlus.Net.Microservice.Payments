namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.SetDefaultCard;

public record SetDefaultCardCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<SetDefaultCardCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
