namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.DeactivateDisbursementRule;

public class DeactivateDisbursementRuleCommandHandler(IDisbursementRuleRepository repository, IUserContext user) : IRequestHandler<DeactivateDisbursementRuleCommand>
{
    public async Task Handle(DeactivateDisbursementRuleCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<DisbursementRuleAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.DisbursementRuleNotFound);

        aggregate.Deactivate();

        await repository.UpdateAsync(aggregate, cancellationToken);
    }
}
