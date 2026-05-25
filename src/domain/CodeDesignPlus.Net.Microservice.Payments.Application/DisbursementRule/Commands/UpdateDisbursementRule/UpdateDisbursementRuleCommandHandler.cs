namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.UpdateDisbursementRule;

public class UpdateDisbursementRuleCommandHandler(IDisbursementRuleRepository repository, IUserContext user) : IRequestHandler<UpdateDisbursementRuleCommand>
{
    public async Task Handle(UpdateDisbursementRuleCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<DisbursementRuleAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.DisbursementRuleNotFound);

        aggregate.Update(
            request.CommissionType,
            request.CommissionAmount,
            request.Currency,
            request.Description
        );

        await repository.UpdateAsync(aggregate, cancellationToken);
    }
}
