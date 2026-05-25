namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Commands.CompleteDisbursement;

public class CompleteDisbursementCommandHandler(IDisbursementRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<CompleteDisbursementCommand>
{
    public async Task Handle(CompleteDisbursementCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<DisbursementAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.DisbursementNotFound);

        aggregate.Complete(request.ProviderReference);

        await repository.UpdateAsync(aggregate, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
