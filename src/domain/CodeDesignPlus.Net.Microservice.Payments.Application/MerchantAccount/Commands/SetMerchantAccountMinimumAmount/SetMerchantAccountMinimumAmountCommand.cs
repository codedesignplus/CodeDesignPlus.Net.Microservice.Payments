using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.SetMerchantAccountMinimumAmount;

/// <summary>
/// Sets the smallest payment the tenant in session accepts online, or clears it (pendings/660).
/// </summary>
/// <param name="MinimumAmount">The minimum in major units with its currency, as the user types it (rule 19); null to keep only the provider's.</param>
public record SetMerchantAccountMinimumAmountCommand(MoneyInput? MinimumAmount) : IRequest<MerchantAccountDto>;

/// <summary>
/// Validator of <see cref="SetMerchantAccountMinimumAmountCommand"/>.
/// </summary>
public class Validator : AbstractValidator<SetMerchantAccountMinimumAmountCommand>
{
    /// <summary>
    /// Initializes the rules of the command.
    /// </summary>
    public Validator()
    {
        RuleFor(x => x.MinimumAmount!.Amount).GreaterThan(0m).When(x => x.MinimumAmount is not null);
        RuleFor(x => x.MinimumAmount!.Currency).NotEmpty().Length(3).When(x => x.MinimumAmount is not null);
    }
}

/// <summary>
/// Converts the minimum to minor units and keeps it on the account, checked against the provider's (error 373). The account
/// must exist: it is born with the tenant.
/// </summary>
public class SetMerchantAccountMinimumAmountCommandHandler(IMerchantAccountRepository repository, IUserContext user, IPubSub pubsub, ICurrencyGrpc currencyGrpc, IOptions<PaymentProvidersOptions> options) : IRequestHandler<SetMerchantAccountMinimumAmountCommand, MerchantAccountDto>
{
    /// <summary>
    /// Sets the minimum.
    /// </summary>
    /// <param name="request">The minimum, or null to clear it.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The account with its minimums, without secrets.</returns>
    public async Task<MerchantAccountDto> Handle(SetMerchantAccountMinimumAmountCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(user.Tenant, Errors.TenantIsRequired);

        var tenant = user.Tenant;

        var current = await repository.GetByTenantAsync(tenant, cancellationToken);

        ApplicationGuard.IsNull(current, Errors.MerchantAccountNotFound);

        Money? minimum = null;

        if (request.MinimumAmount is { } input)
        {
            var currency = await currencyGrpc.GetCurrencyAsync(code: input.Currency, cancellationToken: cancellationToken);

            minimum = input.ToMoney(currency.DecimalDigits);
        }

        MerchantAccountAggregate saved = current!;

        await repository.UpdateWithRetryAsync(async () => (await repository.GetByTenantAsync(tenant, cancellationToken))!, account =>
        {
            saved = account;

            return account.SetMinimumAmount(minimum, options.Value.MinimumAmountFor(account.Provider, minimum?.Currency), user.IdUser);
        }, cancellationToken);

        await pubsub.PublishAsync(saved.GetAndClearEvents(), cancellationToken);

        return MerchantAccountDto.From(saved, options.Value);
    }
}
