using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public sealed partial class Amount
{
    [GeneratedRegex(@"^[A-Z]{3}$")]
    private static partial Regex CurrencyRegex();

    public long Value { get; private set; }
    public string Currency { get; private set; } = null!;

    [JsonConstructor]
    private Amount(long value, string currency)
    {
        DomainGuard.IsNullOrEmpty(currency, Errors.CurrencyCannotBeNullOrEmpty);

        DomainGuard.IsFalse(CurrencyRegex().IsMatch(currency), Errors.CurrencyMustBeValidFormat);

        this.Value = value;
        this.Currency = currency;
    }

    public static Amount Create(long value, string currency)
    {
        return new Amount(value, currency);
    }
}
