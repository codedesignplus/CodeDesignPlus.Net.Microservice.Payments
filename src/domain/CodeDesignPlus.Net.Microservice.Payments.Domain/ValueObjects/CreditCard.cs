using System;
using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public partial class CreditCard
{

    [GeneratedRegex(@"^\d{4}/\d{2}$")]
    private static partial Regex ExpirationDateRegex();

    public string Number { get; private set; } = null!;
    public string SecurityCode { get; private set; } = null!;
    public string ExpirationDate { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public sbyte InstallmentsNumber { get; private set; }

    [JsonConstructor]
    private CreditCard(string number, string securityCode, string expirationDate, string name, sbyte installmentsNumber = 1)
    {
        DomainGuard.IsNullOrEmpty(number, Errors.CreditCardNumberCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(number.Length, 20, Errors.CreditCardNumberCannotBeGreaterThan20Characters);
        DomainGuard.IsLessThan(number.Length, 13, Errors.CreditCardNumberCannotBeLessThan13Characters);

        DomainGuard.IsNullOrEmpty(securityCode, Errors.CreditCardSecurityCodeCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(securityCode.Length, 4, Errors.CreditCardSecurityCodeCannotBeGreaterThan4Characters);
        DomainGuard.IsLessThan(securityCode.Length, 1, Errors.CreditCardSecurityCodeCannotBeLessThan3Characters);

        DomainGuard.IsNullOrEmpty(expirationDate, Errors.CreditCardExpirationDateCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(expirationDate.Length, 7, Errors.CreditCardExpirationDateCannotBeGreaterThan7Characters);
        DomainGuard.IsFalse(ExpirationDateRegex().IsMatch(expirationDate), Errors.CreditCardExpirationDateMustBeValidFormat);

        DomainGuard.IsLessThanOrEqual(installmentsNumber, 0, Errors.CreditCardInstallmentsNumberMustBeGreaterThanZero);

        Number = number;
        SecurityCode = securityCode;
        ExpirationDate = expirationDate;
        Name = name;
        InstallmentsNumber =  installmentsNumber;
    }
    
    public static CreditCard Create(string number, string securityCode, string expirationDate, string name)
    {
        return new CreditCard(number, securityCode, expirationDate, name);
    }
}
