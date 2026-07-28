using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.UpdateDisbursementRule;

public class UpdateDisbursementRuleCommandHandler(IDisbursementRuleRepository repository, IUserContext user, ICurrencyGrpc currencyGrpc) : IRequestHandler<UpdateDisbursementRuleCommand>
{
    public async Task Handle(UpdateDisbursementRuleCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<DisbursementRuleAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.DisbursementRuleNotFound);

        long? fixedCommission = null;

        if (request.FixedCommission is not null)
        {
            var currency = await currencyGrpc.GetCurrencyAsync(code: request.FixedCommission.Currency, cancellationToken: cancellationToken);
            fixedCommission = request.FixedCommission.ToMinorUnits(currency.DecimalDigits);
        }

        aggregate.Update(
            request.CommissionType,
            fixedCommission,
            request.FixedCommission?.Currency,
            request.CommissionPercentage.HasValue ? BasisPoints.FromPercentage(request.CommissionPercentage.Value) : null,
            request.Description
        );

        await repository.UpdateAsync(aggregate, cancellationToken);
    }
}
