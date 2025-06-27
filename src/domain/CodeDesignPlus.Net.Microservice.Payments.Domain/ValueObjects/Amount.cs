using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public sealed partial class Amount
{
    [GeneratedRegex(@"^[A-Z]{3}$")]
    private static partial Regex CurrencyRegex();

    public long Value { get; private set; }
    public string? Currency { get; private set; }

    [JsonConstructor]
    private Amount(long value, string currency)
    {
        ApplicationGuard.IsLessThan(value, 0, Errors.AmountValueMustBeGreaterThanZero);

        this.Value = value;
        this.Currency = currency;
    }

    public static Amount Create(long value, string currency)
    {
        return new Amount(value, currency);
    }
}
