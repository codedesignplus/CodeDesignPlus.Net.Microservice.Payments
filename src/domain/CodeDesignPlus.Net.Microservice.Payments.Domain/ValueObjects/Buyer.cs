using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public partial class Buyer
{
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
    public string FullName { get; private set; } = null!;
    public string EmailAddress { get; private set; } = null!;
    public string ContactPhone { get; private set; } = null!;
    public string DniNumber { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;

    [JsonConstructor]
    private Buyer(string fullName, string emailAddress, string contactPhone, Address shippingAddress, string dniNumber)
    {
        DomainGuard.IsNullOrEmpty(fullName, Errors.FullNameCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(fullName.Length, 100, Errors.FullNameCannotBeGreaterThan100Characters);

        DomainGuard.IsNullOrEmpty(emailAddress, Errors.EmailAddressCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(emailAddress.Length, 255, Errors.EmailAddressCannotBeGreaterThan255Characters);
        DomainGuard.IsFalse(EmailRegex().IsMatch(emailAddress), Errors.EmailAddressMustBeValidFormat);

        DomainGuard.IsNullOrEmpty(contactPhone, Errors.ContactPhoneCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(contactPhone.Length, 20, Errors.ContactPhoneCannotBeGreaterThan20Characters);

        DomainGuard.IsNull(shippingAddress, Errors.ShippingAddressCannotBeNullOrEmpty);

        DomainGuard.IsNullOrEmpty(dniNumber, Errors.DniNumberCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(dniNumber.Length, 20, Errors.DniNumberCannotBeGreaterThan20Characters);

        FullName = fullName;
        EmailAddress = emailAddress;
        ContactPhone = contactPhone;
        ShippingAddress = shippingAddress;
        DniNumber = dniNumber;
    }
    
    public static Buyer Create(string fullName, string emailAddress, string contactPhone, Address shippingAddress, string dniNumber)
    {
        return new Buyer(fullName, emailAddress, contactPhone, shippingAddress, dniNumber);
    }

}
