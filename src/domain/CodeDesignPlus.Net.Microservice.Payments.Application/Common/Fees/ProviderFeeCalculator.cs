using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Fees;

/// <summary>
/// Knows the commission the provider keeps from a collected payment.
/// </summary>
public interface IProviderFeeCalculator
{
    /// <summary>
    /// The commission of a succeeded payment: the one the provider reported, if it did; otherwise the one computed with
    /// the rates of the collection account that collected it.
    /// </summary>
    /// <param name="payment">The payment.</param>
    /// <param name="captured">What the provider captured.</param>
    /// <param name="paymentMethodType">The method the payer chose, which may have its own rate.</param>
    /// <param name="reported">What the provider reported, when it does.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The commission, or null when it cannot be known.</returns>
    Task<ProviderFee?> ForAsync(PaymentAggregate payment, Money? captured, string? paymentMethodType, ProviderFee? reported, CancellationToken cancellationToken);
}

/// <summary>
/// Computes the commission with the rates of the tenant's collection account, or with the provider's default of the
/// deployment when the tenant has not set them; the platform's own account always pays the default.
/// </summary>
/// <remarks>
/// What the provider reports prevails. Any difference against the calculation is logged, with no tolerance: it means
/// the rates of the account are not the ones of the tenant's contract.
/// </remarks>
public sealed class ProviderFeeCalculator(IMerchantAccountRepository accounts, IOptions<PaymentProvidersOptions> options, ILogger<ProviderFeeCalculator> logger) : IProviderFeeCalculator
{
    /// <inheritdoc/>
    public async Task<ProviderFee?> ForAsync(PaymentAggregate payment, Money? captured, string? paymentMethodType, ProviderFee? reported, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payment);

        if (captured is null)
        {
            return null;
        }

        var calculated = await CalculateAsync(payment, captured, paymentMethodType, cancellationToken);

        if (reported is null)
        {
            return calculated;
        }

        if (calculated is not null && (calculated.Fee.Amount != reported.Fee.Amount || calculated.Tax.Amount != reported.Tax.Amount))
        {
            logger.LogWarning(
                "Payment {PaymentId} of tenant {Tenant}: the provider reported a commission of {ReportedFee} + {ReportedTax} {Currency} and the rates of the account give {CalculatedFee} + {CalculatedTax}. The reported one is kept; check the rates of the account.",
                payment.Id, payment.Tenant, reported.Fee.Amount, reported.Tax.Amount, captured.Currency, calculated.Fee.Amount, calculated.Tax.Amount);
        }

        return reported with { Source = ProviderFeeSources.Reported };
    }

    private async Task<ProviderFee?> CalculateAsync(PaymentAggregate payment, Money captured, string? paymentMethodType, CancellationToken cancellationToken)
    {
        var schedule = payment.PlatformAccount || payment.Tenant is null
            ? null
            : (await accounts.GetByTenantAsync(payment.Tenant.Value, cancellationToken))?.FeeSchedule;

        schedule ??= options.Value.DefaultFeeFor(payment.Provider);

        if (schedule is null)
        {
            logger.LogWarning("Payment {PaymentId}: there are no rates for provider {Provider}, so its commission is not known.", payment.Id, payment.Provider);

            return null;
        }

        var fee = schedule.RateFor(paymentMethodType).Apply(captured);

        if (fee is null)
        {
            logger.LogWarning("Payment {PaymentId}: the fixed amount of the rate is not in {Currency}, so its commission is not known.", payment.Id, captured.Currency);
        }

        return fee;
    }
}
