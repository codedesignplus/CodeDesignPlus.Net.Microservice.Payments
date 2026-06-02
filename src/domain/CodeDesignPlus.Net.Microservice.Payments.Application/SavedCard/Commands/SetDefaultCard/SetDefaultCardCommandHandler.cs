namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.SetDefaultCard;

public class SetDefaultCardCommandHandler(ISavedCardRepository repository, IUserContext user) : IRequestHandler<SetDefaultCardCommand>
{
    public async Task Handle(SetDefaultCardCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<SavedCardAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.SavedCardNotFound);

        // Unset any existing default cards for this user
        var userCards = await repository.GetByUserIdAsync(user.IdUser, cancellationToken);

        foreach (var card in userCards.Where(c => c.IsDefault && c.Id != request.Id))
        {
            card.UnsetDefault();
            await repository.UpdateAsync(card, cancellationToken);
        }

        // Set the requested card as default
        aggregate.SetAsDefault();

        await repository.UpdateAsync(aggregate, cancellationToken);
    }
}
