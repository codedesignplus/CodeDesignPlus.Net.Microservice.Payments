using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public partial class Address
{
    [GeneratedRegex(@"^[A-Z]{2}$")]
    private static partial Regex CountryRegex();

    [GeneratedRegex(@"^\d{1,8}$")]
    private static partial Regex PostalCodeRegex();

    [GeneratedRegex(@"^\+?\d{1,11}$")]
    private static partial Regex PhoneRegex();

    public string Street { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string PostalCode { get; private set; } = null!;
    public string Phone { get; private set; } = null!;

    [JsonConstructor]
    private Address(string street, string country, string state, string city, string postalCode, string phone)
    {
        DomainGuard.IsNullOrEmpty(street, Errors.StreetCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(street.Length, 100, Errors.StreetCannotBeGreaterThan100Characters);

        DomainGuard.IsNullOrEmpty(country, Errors.CountryCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(country.Length, 2, Errors.CountryCannotBeGreaterThan2Characters);
        DomainGuard.IsFalse(CountryRegex().IsMatch(country), Errors.CountryMustBeTwoUppercaseLetters);

        DomainGuard.IsNullOrEmpty(state, Errors.StateCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(state.Length, 40, Errors.StateCannotBeGreaterThan40Characters);

        DomainGuard.IsNullOrEmpty(city, Errors.CityCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(city.Length, 50, Errors.CityCannotBeGreaterThan50Characters);

        DomainGuard.IsNullOrEmpty(postalCode, Errors.PostalCodeCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(postalCode.Length, 8, Errors.PostalCodeCannotBeGreaterThan8Characters);
        DomainGuard.IsFalse(PostalCodeRegex().IsMatch(postalCode), Errors.PostalCodeMustBeValidFormat);

        DomainGuard.IsNullOrEmpty(phone, Errors.PhoneCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(phone.Length, 11, Errors.PhoneCannotBeGreaterThan11Characters);
        DomainGuard.IsFalse(PhoneRegex().IsMatch(phone), Errors.PhoneMustBeValidFormat);

        Street = street;
        Country = country;
        State = state;
        City = city;
        PostalCode = postalCode;
        Phone = phone;
    }

    public static Address Create(string street, string country, string state, string city, string postalCode, string phone)
    {
        return new Address(street, country, state, city, postalCode, phone);
    }
}
