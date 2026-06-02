namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.DeleteSavedCard;

public class DeleteSavedCardCommandHandler(ISavedCardRepository repository, IUserContext user, IPubSub pubSub) : IRequestHandler<DeleteSavedCardCommand>
{
    public async Task Handle(DeleteSavedCardCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<SavedCardAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.SavedCardNotFound);

        aggregate.Delete();

        await repository.UpdateAsync(aggregate, cancellationToken);

        await pubSub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
