namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.DeleteSavedCard;

public class DeleteSavedCardCommandHandler(ISavedCardRepository repository, IUserContext user, IPubSub pubSub) : IRequestHandler<DeleteSavedCardCommand>
{
    public async Task Handle(DeleteSavedCardCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<SavedCardAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.SavedCardNotFound);

        // Una tarjeta ajena responde igual que una que no existe: no se revela que existe (pendings/180).
        ApplicationGuard.IsTrue(aggregate.UserId != user.IdUser, Errors.SavedCardNotFound);

        aggregate.Delete(user.IdUser);

        // Una tarjeta es un dato de pago: al borrarla se borra del todo, también el token de la pasarela.
        await repository.DeleteAsync<SavedCardAggregate>(aggregate.Id, cancellationToken);

        await pubSub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
