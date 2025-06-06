namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class Errors : IErrorCodes
{
    public const string UnknownError = "100 : UnknownError";
    public const string DescriptionCannotBeNullOrEmpty = "101 : Description cannot be null or empty";
    public const string DescriptionCannotBeGreaterThan255Characters = "102 : Description cannot be greater than 255 characters";
    public const string BuyerCannotBeNull = "103 : Buyer cannot be null";
    public const string StreetCannotBeNullOrEmpty = "104 : Street cannot be null or empty";
    public const string StreetCannotBeGreaterThan100Characters = "105 : Street cannot be greater than 100 characters";
    public const string CountryCannotBeNullOrEmpty = "106 : Country cannot be null or empty";
    public const string CountryCannotBeGreaterThan2Characters = "107 : Country cannot be greater than 2 characters";
    public const string CountryMustBeTwoUppercaseLetters = "108 : Country must be two uppercase letters";
    public const string StateCannotBeNullOrEmpty = "109 : State cannot be null or empty";
    public const string StateCannotBeGreaterThan40Characters = "110 : State cannot be greater than 40 characters";
    public const string CityCannotBeNullOrEmpty = "111 : City cannot be null or empty";
    public const string CityCannotBeGreaterThan50Characters = "112 : City cannot be greater than 50 characters";
    public const string PostalCodeCannotBeNullOrEmpty = "113 : PostalCode cannot be null or empty";
    public const string PostalCodeCannotBeGreaterThan8Characters = "114 : PostalCode cannot be greater than 8 characters";
    public const string PostalCodeMustBeValidFormat = "115 : PostalCode must be valid format";
    public const string PhoneCannotBeNullOrEmpty = "116 : Phone cannot be null or empty";
    public const string PhoneCannotBeGreaterThan11Characters = "117 : Phone cannot be greater than 11 characters";
    public const string PhoneMustBeValidFormat = "118 : Phone must be valid format";
    public const string FullNameCannotBeNullOrEmpty = "119 : FullName cannot be null or empty";
    public const string FullNameCannotBeGreaterThan100Characters = "120 : FullName cannot be greater than 100 characters";
    public const string EmailAddressCannotBeNullOrEmpty = "121 : EmailAddress cannot be null or empty";
    public const string EmailAddressCannotBeGreaterThan255Characters = "122 : EmailAddress cannot be greater than 255 characters";
    public const string EmailAddressMustBeValidFormat = "123 : EmailAddress must be valid format";
    public const string ContactPhoneCannotBeNullOrEmpty = "124 : ContactPhone cannot be null or empty";
    public const string ContactPhoneCannotBeGreaterThan20Characters = "125 : ContactPhone cannot be greater than 20 characters";
    public const string ShippingAddressCannotBeNullOrEmpty = "126 : ShippingAddress cannot be null or empty";
    public const string DniNumberCannotBeNullOrEmpty = "127 : DniNumber cannot be null or empty";
    public const string DniNumberCannotBeGreaterThan20Characters = "128 : DniNumber cannot be greater than 20 characters";
    public const string FullNameCannotBeGreaterThan150Characters = "129 : FullName cannot be greater than 150 characters";
    public const string BillingAddressCannotBeNull = "130 : BillingAddress cannot be null";
    public const string OrderCannotBeNull = "131 : Order cannot be null";
    public const string PaymentMethodCannotBeNullOrEmpty = "132 : PaymentMethod cannot be null or empty";
    public const string PaymentMethodCannotBeGreaterThan32Characters = "133 : PaymentMethod cannot be greater than 32 characters";
    public const string CreditCardCannotBeNull = "134 : CreditCard cannot be null";

    public const string ProviderCannotBeNullOrEmpty = "135 : Provider cannot be null or empty"; 
    public const string ProviderCannotBeGreaterThan50Characters = "136 : Provider cannot be greater than 50 characters"; 
    public const string TransactionCannotBeNull = "137 : Transaction cannot be null"; 
    public const string RequestCannotBeNullOrEmpty = "138 : Request cannot be null or empty"; 
    public const string ResponseCannotBeNullOrEmpty = "139 : Response cannot be null or empty";

    public const string CurrencyCannotBeNullOrEmpty = "140 : Currency cannot be null or empty"; 
    public const string CurrencyMustBeValidFormat = "141 : Currency must be valid format";

    public const string DeviceSessionIdMustBeValidFormat = "142 : Device SessionId must be valid format";
    public const string DeviceSessionIdCannotBeNullOrEmpty = "143 : Device SessionId cannot be null or empty";

    public const string CreditCardNumberCannotBeNullOrEmpty  = "144 : Credit Card Number cannot be null or empty";
    public const string CreditCardNumberCannotBeGreaterThan20Characters  = "145 : Credit Card Number cannot be greater than 20 characters"; 
    public const string CreditCardNumberCannotBeLessThan13Characters = "146 : Credit Card Number cannot be less than 13 characters"; 
    public const string CreditCardSecurityCodeCannotBeNullOrEmpty = "147 : Credit Card Security Code cannot be null or empty"; 
    public const string CreditCardSecurityCodeCannotBeGreaterThan4Characters = "148 : Credit Card Security Code cannot be greater than 4 characters"; 
    public const string CreditCardSecurityCodeCannotBeLessThan3Characters = "149 : Credit Card Security Code cannot be less than 3 characters"; 
    public const string CreditCardExpirationDateCannotBeNullOrEmpty = "150 : Credit Card Expiration Date cannot be null or empty"; 
    public const string CreditCardExpirationDateMustBeValidFormat = "151 : Credit Card Expiration Date must be valid format";

    public const string CreditCardExpirationDateCannotBeGreaterThan7Characters = "152 : Credit Card Expiration Date cannot be greater than 7 characters";

    public const string DeviceSessionIdCannotBeGreaterThan255Characters = "153 : Device SessionId cannot be greater than 255 characters"; 
    public const string IpAddressCannotBeNullOrEmpty = "154 : IP Address cannot be null or empty"; 
    public const string IpAddressCannotBeGreaterThan39Characters = "155 : IP Address cannot be greater than 39 characters"; 
    public const string IpAddressMustBeValidFormat = "156 : IP Address must be valid format"; 
    public const string CookieCannotBeNullOrEmpty = "157 : Cookie cannot be null or empty"; 
    public const string CookieCannotBeGreaterThan255Characters = "158 : Cookie cannot be greater than 255 characters"; 
    public const string UserAgentCannotBeNullOrEmpty = "159 : User Agent cannot be null or empty"; 
    public const string UserAgentCannotBeGreaterThan1024Characters = "160 : User Agent cannot be greater than 1024 characters";

    public const string AmountCannotBeNull = "161 : Amount cannot be null";
    public const string TaxCannotBeNull = "162 : Tax cannot be null"; 
    public const string TaxReturnBaseCannotBeNull = "163 : Tax Return Base cannot be null"; 
}
