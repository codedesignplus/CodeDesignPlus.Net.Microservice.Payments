using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public sealed partial record Pse
{
    [GeneratedRegex(@"^https?:\/\/([a-zA-Z0-9\-\.]+)(:[0-9]+)?(\/[^\s]*)?$", RegexOptions.Compiled)]
    private static partial Regex UrlRegex();

    public string PseCode { get; init; }
    public string TypePerson { get; init; }
    public string PseResponseUrl { get; init; }

    [JsonConstructor]
    private Pse(string pseCode, string typePerson, string pseResponseUrl)
    {
        var normalizedPseCode = pseCode?.Trim() ?? string.Empty;
        var normalizedTypePerson = typePerson?.Trim().ToUpperInvariant() ?? string.Empty;
        var normalizedUrl = pseResponseUrl?.Trim() ?? string.Empty;

        DomainGuard.IsNullOrEmpty(normalizedPseCode, Errors.PseCodeCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(normalizedPseCode.Length, 34, Errors.PseCodeCannotBeGreaterThan34Characters);

        DomainGuard.IsNullOrEmpty(normalizedTypePerson, Errors.TypePersonCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(normalizedTypePerson.Length, 1, Errors.TypePersonCannotBeGreaterThan1Character);

        DomainGuard.IsNullOrEmpty(normalizedUrl, Errors.PseResponseUrlCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(normalizedUrl.Length, 255, Errors.PseResponseUrlCannotBeGreaterThan255Characters);
        DomainGuard.IsFalse(UrlRegex().IsMatch(normalizedUrl), Errors.PseResponseUrlMustBeValidFormat);

        PseCode = normalizedPseCode;
        TypePerson = normalizedTypePerson;
        PseResponseUrl = normalizedUrl;
    }

    public static Pse Create(string pseCode, string typePerson, string pseResponseUrl)
    {
        return new Pse(pseCode, typePerson, pseResponseUrl);
    }
}
