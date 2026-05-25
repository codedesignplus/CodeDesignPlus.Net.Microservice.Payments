namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.CreateDisbursementRule;

public class CreateDisbursementRuleCommandHandler(IDisbursementRuleRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<CreateDisbursementRuleCommand>
{
    public async Task Handle(CreateDisbursementRuleCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exist = await repository.ExistsAsync<DisbursementRuleAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.DisbursementRuleAlreadyExists);

        var aggregate = DisbursementRuleAggregate.Create(
            request.Id,
            request.CommissionType,
            request.CommissionAmount,
            request.Currency,
            request.Description,
            user.Tenant
        );

        await repository.CreateAsync(aggregate, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
