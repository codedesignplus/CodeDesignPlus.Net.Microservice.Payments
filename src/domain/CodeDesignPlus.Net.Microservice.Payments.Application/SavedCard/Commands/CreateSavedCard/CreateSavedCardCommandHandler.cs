namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.CreateSavedCard;

public class CreateSavedCardCommandHandler(ISavedCardRepository repository, IUserContext user, IPubSub pubSub) : IRequestHandler<CreateSavedCardCommand>
{
    public async Task Handle(CreateSavedCardCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        // Ya guardado por un envío anterior del mismo formulario: el reintento termina bien sin repetir nada (pendings/589).
        if (await repository.ExistsAsync<SavedCardAggregate>(request.Id, cancellationToken))
            return;

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

        // Dos envíos que llegan a la vez pasan los dos la comprobación de arriba; el segundo choca contra el id y no publica.
        if (!await repository.CreateIfAbsentAsync(aggregate, cancellationToken))
            return;

        await pubSub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
