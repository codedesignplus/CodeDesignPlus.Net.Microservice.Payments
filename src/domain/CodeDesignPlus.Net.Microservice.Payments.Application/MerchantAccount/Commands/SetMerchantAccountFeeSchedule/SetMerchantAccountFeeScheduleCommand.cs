using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.SetMerchantAccountFeeSchedule;

/// <summary>
/// Sets the rates the provider charges the tenant in session, as its contract with the provider says.
/// </summary>
/// <param name="Default">The rate of every payment method without its own.</param>
/// <param name="ByPaymentMethod">The payment methods with a rate of their own; null or empty when there are none.</param>
public record SetMerchantAccountFeeScheduleCommand(ProviderFeeRateInput Default, List<PaymentMethodFeeRateInput>? ByPaymentMethod) : IRequest<MerchantAccountDto>;

/// <summary>
/// A rate as the user types it: percentages and the fixed amount in major units (rules 04 and 05).
/// </summary>
/// <param name="RatePercentage">Percentage of the gross (2.65 for 2,65 %).</param>
/// <param name="FixedAmount">Fixed amount per payment, in major units with its currency.</param>
/// <param name="TaxRatePercentage">Tax on the commission (19 for 19 %).</param>
public record ProviderFeeRateInput(decimal RatePercentage, MoneyInput FixedAmount, decimal TaxRatePercentage);

/// <summary>
/// The rate of one payment method.
/// </summary>
/// <param name="PaymentMethodType">Payment method as the provider names it (for example <c>CARD</c>).</param>
/// <param name="Rate">Its rate.</param>
public record PaymentMethodFeeRateInput(string PaymentMethodType, ProviderFeeRateInput Rate);

public class Validator : AbstractValidator<SetMerchantAccountFeeScheduleCommand>
{
    public Validator()
    {
        RuleFor(x => x.Default).NotNull().SetValidator(new RateValidator());
        RuleForEach(x => x.ByPaymentMethod).ChildRules(method =>
        {
            method.RuleFor(x => x.PaymentMethodType).NotEmpty().MaximumLength(50);
            method.RuleFor(x => x.Rate).NotNull().SetValidator(new RateValidator());
        });
    }

    private sealed class RateValidator : AbstractValidator<ProviderFeeRateInput>
    {
        public RateValidator()
        {
            RuleFor(x => x.RatePercentage).InclusiveBetween(0m, 100m);
            RuleFor(x => x.TaxRatePercentage).InclusiveBetween(0m, 100m);
            RuleFor(x => x.FixedAmount).NotNull();
            RuleFor(x => x.FixedAmount.Amount).GreaterThanOrEqualTo(0m).When(x => x.FixedAmount is not null);
            RuleFor(x => x.FixedAmount.Currency).NotEmpty().Length(3).When(x => x.FixedAmount is not null);
        }
    }
}

/// <summary>
/// Converts the rates to basis points and minor units and keeps them on the account. The account must exist: it is born
/// with the tenant, or when a tenant created before collection accounts connects its keys.
/// </summary>
public class SetMerchantAccountFeeScheduleCommandHandler(IMerchantAccountRepository repository, IUserContext user, IPubSub pubsub, ICurrencyGrpc currencyGrpc, IOptions<PaymentProvidersOptions> options) : IRequestHandler<SetMerchantAccountFeeScheduleCommand, MerchantAccountDto>
{
    public async Task<MerchantAccountDto> Handle(SetMerchantAccountFeeScheduleCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(user.Tenant, Errors.TenantIsRequired);

        var tenant = user.Tenant;

        var current = await repository.GetByTenantAsync(tenant, cancellationToken);

        ApplicationGuard.IsNull(current, Errors.MerchantAccountNotFound);

        var methods = new List<PaymentMethodFeeRate>();

        foreach (var method in request.ByPaymentMethod ?? [])
        {
            methods.Add(new PaymentMethodFeeRate(method.PaymentMethodType, await ToRateAsync(method.Rate, cancellationToken)));
        }

        var schedule = ProviderFeeSchedule.Create(await ToRateAsync(request.Default, cancellationToken), methods);

        MerchantAccountAggregate saved = null!;

        await repository.UpdateWithRetryAsync(async () => (await repository.GetByTenantAsync(tenant, cancellationToken))!, account =>
        {
            account.SetFeeSchedule(schedule, user.IdUser);

            saved = account;

            return true;
        }, cancellationToken);

        await pubsub.PublishAsync(saved.GetAndClearEvents(), cancellationToken);

        return MerchantAccountDto.From(saved, options.Value);
    }

    private async Task<ProviderFeeRate> ToRateAsync(ProviderFeeRateInput input, CancellationToken cancellationToken)
    {
        var currency = await currencyGrpc.GetCurrencyAsync(code: input.FixedAmount.Currency, cancellationToken: cancellationToken);

        return ProviderFeeRate.Create(BasisPoints.FromPercentage(input.RatePercentage), input.FixedAmount.ToMoney(currency.DecimalDigits), BasisPoints.FromPercentage(input.TaxRatePercentage));
    }
}
