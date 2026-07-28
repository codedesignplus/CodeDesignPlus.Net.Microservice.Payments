using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Commands.CreateDisbursementRule;

public class CreateDisbursementRuleCommandHandler(IDisbursementRuleRepository repository, IUserContext user, IPubSub pubsub, ICurrencyGrpc currencyGrpc) : IRequestHandler<CreateDisbursementRuleCommand>
{
    public async Task Handle(CreateDisbursementRuleCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exist = await repository.ExistsAsync<DisbursementRuleAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.DisbursementRuleAlreadyExists);

        long? fixedCommission = null;

        if (request.FixedCommission is not null)
        {
            var currency = await currencyGrpc.GetCurrencyAsync(code: request.FixedCommission.Currency, cancellationToken: cancellationToken);
            fixedCommission = request.FixedCommission.ToMinorUnits(currency.DecimalDigits);
        }

        var aggregate = DisbursementRuleAggregate.Create(
            request.Id,
            request.CommissionType,
            fixedCommission,
            request.FixedCommission?.Currency,
            request.CommissionPercentage.HasValue ? BasisPoints.FromPercentage(request.CommissionPercentage.Value) : null,
            request.Description,
            user.Tenant
        );

        await repository.CreateAsync(aggregate, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
