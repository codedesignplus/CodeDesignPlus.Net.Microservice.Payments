using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public partial class Payer
{
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();

    public string EmailAddress { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public Address BillingAddress { get; private set; } = null!;
    public string DniNumber { get; private set; } = null!;
    public string ContactPhone { get; private set; } = null!;
    public string DniType { get; private set; } = null!;

    [JsonConstructor]
    private Payer(string fullName, string emailAddress, string contactPhone, string dniNumber, string dniType, Address billingAddress)
    {
        DomainGuard.IsNullOrEmpty(emailAddress, Errors.EmailAddressCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(emailAddress.Length, 255, Errors.EmailAddressCannotBeGreaterThan255Characters);
        DomainGuard.IsFalse(EmailRegex().IsMatch(emailAddress), Errors.EmailAddressMustBeValidFormat);

        DomainGuard.IsNullOrEmpty(fullName, Errors.FullNameCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(fullName.Length, 150, Errors.FullNameCannotBeGreaterThan150Characters);

        DomainGuard.IsNullOrEmpty(dniNumber, Errors.DniNumberCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(dniNumber.Length, 20, Errors.DniNumberCannotBeGreaterThan20Characters);

        DomainGuard.IsNullOrEmpty(contactPhone, Errors.ContactPhoneCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(contactPhone.Length, 20, Errors.ContactPhoneCannotBeGreaterThan20Characters);

        DomainGuard.IsNullOrEmpty(dniType, Errors.DniTypeCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(dniType.Length, 3, Errors.DniTypeCannotBeGreaterThan3Characters);

        EmailAddress = emailAddress;
        FullName = fullName;
        BillingAddress = billingAddress;
        DniNumber = dniNumber;
        ContactPhone = contactPhone;
        DniType = dniType;
    }

    public static Payer Create(string fullName, string emailAddress, string contactPhone, string dniNumber, string dniType, Address billingAddress)
    {
        return new Payer(fullName, emailAddress, contactPhone, dniNumber, dniType, billingAddress);
    }
}
