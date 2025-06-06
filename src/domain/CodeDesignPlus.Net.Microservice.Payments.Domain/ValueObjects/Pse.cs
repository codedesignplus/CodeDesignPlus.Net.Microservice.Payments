using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public sealed partial class Pse
{
    [GeneratedRegex(@"^0x[0-9]{32}$")]
    private static partial Regex Regex();

    public string FullName { get; private set; } = null!;
    public string PseCode { get; private set; } = null!;
    public string TypePerson { get; private set; } = null!;
    public string TypeDocument { get; private set; } = null!;
    public string Document { get; private set; } = null!;

    [JsonConstructor]
    private Pse(string value)
    {
        DomainGuard.IsNullOrEmpty(value, Errors.UnknownError);

        DomainGuard.IsFalse(Regex().IsMatch(value), Errors.UnknownError);

        this.PseCode = value;
    }

    public static Pse Create(string value)
    {
        return new Pse(value);
    }
}
