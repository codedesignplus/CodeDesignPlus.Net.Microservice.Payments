namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.DeleteSavedCard;

public record DeleteSavedCardCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<DeleteSavedCardCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
