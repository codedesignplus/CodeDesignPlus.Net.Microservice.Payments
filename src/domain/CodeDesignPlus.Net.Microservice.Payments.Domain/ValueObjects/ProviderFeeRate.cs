using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

/// <summary>
/// What the provider charges the merchant for each collected payment: a percentage of the gross, a fixed amount, and
/// the tax on that commission.
/// </summary>
/// <remarks>
/// The rates are basis points and the fixed amount is money in minor units of its currency (rules 04 and 05), so the
/// commission is computed without floating point drift.
/// </remarks>
/// <param name="RateBasisPoints">Percentage of the gross, in basis points (265 = 2.65 %).</param>
/// <param name="FixedAmount">Fixed amount per payment, in minor units of its currency.</param>
/// <param name="TaxRateBasisPoints">Tax on the commission, in basis points (1900 = 19 %).</param>
public sealed record ProviderFeeRate(int RateBasisPoints, Money FixedAmount, int TaxRateBasisPoints)
{
    /// <summary>
    /// Creates a rate after checking it.
    /// </summary>
    /// <param name="rateBasisPoints">Percentage of the gross, in basis points.</param>
    /// <param name="fixedAmount">Fixed amount per payment.</param>
    /// <param name="taxRateBasisPoints">Tax on the commission, in basis points.</param>
    /// <returns>The rate.</returns>
    public static ProviderFeeRate Create(int rateBasisPoints, Money fixedAmount, int taxRateBasisPoints)
    {
        DomainGuard.IsNull(fixedAmount, Errors.ProviderFeeFixedAmountIsRequired);
        DomainGuard.IsTrue(rateBasisPoints < 0 || taxRateBasisPoints < 0 || fixedAmount.Amount < 0, Errors.ProviderFeeRateCannotBeNegative);

        return new ProviderFeeRate(rateBasisPoints, fixedAmount, taxRateBasisPoints);
    }

    /// <summary>
    /// The commission and its tax on a collected payment.
    /// </summary>
    /// <remarks>
    /// Each part is rounded half away from zero to the minor unit, the commission before its tax, as the provider bills
    /// it: with Wompi, $10.000 gives $965 of commission and $183,35 of tax, which is what it held back on 29-oct-2026.
    /// </remarks>
    /// <param name="gross">What the provider captured.</param>
    /// <returns>The commission, or null when the fixed amount is in another currency and cannot be added.</returns>
    public ProviderFee? Apply(Money gross)
    {
        ArgumentNullException.ThrowIfNull(gross);

        if (!string.Equals(gross.Currency, FixedAmount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var fee = Percentage(gross.Amount, RateBasisPoints) + FixedAmount.Amount;
        var tax = Percentage(fee, TaxRateBasisPoints);

        return new ProviderFee(Money.FromLong(fee, gross.Currency), Money.FromLong(tax, gross.Currency), ProviderFeeSources.Calculated);
    }

    private static long Percentage(long amount, int basisPoints) => (long)Math.Round(amount * (decimal)basisPoints / 10_000m, MidpointRounding.AwayFromZero);
}

/// <summary>
/// The rate the provider charges for one payment method, when it differs from the general one.
/// </summary>
/// <param name="PaymentMethodType">Payment method as the provider names it (for example <c>CARD</c> or <c>PSE</c>).</param>
/// <param name="Rate">The rate of that method.</param>
public sealed record PaymentMethodFeeRate(string PaymentMethodType, ProviderFeeRate Rate);

/// <summary>
/// The rates a merchant pays its provider: a general one and, optionally, one per payment method.
/// </summary>
/// <param name="Default">The rate of every method without its own.</param>
/// <param name="ByPaymentMethod">The methods with a rate of their own.</param>
public sealed record ProviderFeeSchedule(ProviderFeeRate Default, List<PaymentMethodFeeRate> ByPaymentMethod)
{
    /// <summary>
    /// Creates a schedule after checking it.
    /// </summary>
    /// <param name="default">The general rate.</param>
    /// <param name="byPaymentMethod">The methods with a rate of their own, or null when there are none.</param>
    /// <returns>The schedule.</returns>
    public static ProviderFeeSchedule Create(ProviderFeeRate @default, IEnumerable<PaymentMethodFeeRate>? byPaymentMethod)
    {
        DomainGuard.IsNull(@default, Errors.ProviderFeeScheduleIsRequired);

        var methods = byPaymentMethod?.ToList() ?? [];

        DomainGuard.IsTrue(methods.Exists(x => x is null || x.Rate is null || string.IsNullOrWhiteSpace(x.PaymentMethodType)), Errors.PaymentMethodTypeIsRequired);

        var normalized = methods
            .Select(x => new PaymentMethodFeeRate(x.PaymentMethodType.Trim().ToUpperInvariant(), x.Rate))
            .GroupBy(x => x.PaymentMethodType, StringComparer.Ordinal)
            .Select(x => x.Last())
            .ToList();

        return new ProviderFeeSchedule(@default, normalized);
    }

    /// <summary>
    /// The rate of a payment method: its own if it has one, the general one otherwise.
    /// </summary>
    /// <param name="paymentMethodType">The method the payer chose, or null when the provider did not say.</param>
    /// <returns>The rate.</returns>
    public ProviderFeeRate RateFor(string? paymentMethodType)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodType))
        {
            return Default;
        }

        var own = (ByPaymentMethod ?? []).FirstOrDefault(x => string.Equals(x.PaymentMethodType, paymentMethodType.Trim(), StringComparison.OrdinalIgnoreCase));

        return own?.Rate ?? Default;
    }
}

/// <summary>
/// The commission the provider keeps from a collected payment, and its tax.
/// </summary>
/// <param name="Fee">The commission, in minor units.</param>
/// <param name="Tax">The tax on the commission, in minor units.</param>
/// <param name="Source">Whether it was computed from the configured rate or reported by the provider (<see cref="ProviderFeeSources"/>).</param>
public sealed record ProviderFee(Money Fee, Money Tax, string Source)
{
    /// <summary>
    /// What the merchant receives of a gross: the gross less the commission and its tax.
    /// </summary>
    /// <param name="gross">What the provider captured.</param>
    /// <returns>The expected net.</returns>
    public Money NetOf(Money gross) => Money.FromLong(gross.Amount - Fee.Amount - Tax.Amount, gross.Currency);
}

/// <summary>
/// Where a commission came from. Text and not an enum, so it travels in the events as it reads.
/// </summary>
public static class ProviderFeeSources
{
    /// <summary>
    /// Computed with the rate configured on the collection account.
    /// </summary>
    public const string Calculated = "Calculated";

    /// <summary>
    /// Reported by the provider for the transaction; it prevails over the calculated one.
    /// </summary>
    public const string Reported = "Reported";
}
