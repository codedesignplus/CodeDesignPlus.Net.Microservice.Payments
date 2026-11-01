using PhoneNumbers;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;

/// <summary>
/// Splits a buyer's phone into the country prefix and the national number that the Wompi checkout asks for apart.
/// </summary>
/// <remarks>
/// The phone comes in international format, as the users service stores it (+573019777777). The split uses libphonenumber,
/// the standard numbering plan of every country, so no list of prefixes is written here. A phone without its prefix cannot be
/// split without guessing the country: it is not sent, and the payer types it on the page.
/// </remarks>
public static class WompiPhone
{
    /// <summary>
    /// Splits the phone.
    /// </summary>
    /// <param name="phone">The phone in international format.</param>
    /// <param name="prefix">The country prefix with its plus sign, for example «+57».</param>
    /// <param name="number">The national number, digits only, for example «3019777777».</param>
    /// <returns><c>true</c> if the phone could be split.</returns>
    public static bool TrySplit(string? phone, out string prefix, out string number)
    {
        prefix = string.Empty;
        number = string.Empty;

        if (string.IsNullOrWhiteSpace(phone) || !phone.TrimStart().StartsWith('+'))
        {
            return false;
        }

        try
        {
            var parsed = PhoneNumberUtil.GetInstance().Parse(phone, null);

            if (parsed.CountryCode <= 0 || parsed.NationalNumber == 0)
            {
                return false;
            }

            prefix = $"+{parsed.CountryCode}";
            number = PhoneNumberUtil.GetInstance().GetNationalSignificantNumber(parsed);

            return true;
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
