namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.CreateSavedCard;

public class CreateSavedCardCommandHandler(ISavedCardRepository repository, IUserContext user, IPubSub pubSub) : IRequestHandler<CreateSavedCardCommand>
{
    public async Task Handle(CreateSavedCardCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exist = await repository.ExistsAsync<SavedCardAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.SavedCardAlreadyExists);

        var aggregate = SavedCardAggregate.Create(
            request.Id,
            user.IdUser,
            request.Token,
            request.MaskedNumber,
            request.Franchise,
            request.CardHolderName,
            request.ExpirationDate,
            request.Last4Digits,
            user.IdUser
        );

        await repository.CreateAsync(aggregate, cancellationToken);

        await pubSub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
