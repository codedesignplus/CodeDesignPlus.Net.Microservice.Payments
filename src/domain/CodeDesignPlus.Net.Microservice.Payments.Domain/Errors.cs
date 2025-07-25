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
    public const string ShippingAddressCannotBeNullOrEmpty = "126 : ð€ä³å4ter   ";
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
    public const string RequestCannotBeNull = "138 : Request cannot be null";
    public const string ResponseCannotBeNull = "139 : Response cannot be null";

    public const string CurrencyCannotBeNullOrEmpty = "140 : Currency cannot be null or empty";
    public const string CurrencyMustBeValidFormat = "141 : Currency must be valid format";

    public const string DeviceSessionIdMustBeValidFormat = "142 : Device SessionId must be valid format";
    public const string DeviceSessionIdCannotBeNullOrEmpty = "143 : Device SessionId cannot be null or empty";

    public const string CreditCardNumberCannotBeNullOrEmpty = "144 : Credit Card Number cannot be null or empty";
    public const string CreditCardNumberCannotBeGreaterThan20Characters = "145 : Credit Card Number cannot be greater than 20 characters";
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

    public const string DniTypeCannotBeNullOrEmpty = "164 : Dni Type cannot be null or empty";
    public const string DniTypeCannotBeGreaterThan3Characters = "165 : Dni Type cannot be greater than 3 characters";

    public const string PseCodeCannotBeNullOrEmpty = "166 : Pse Code cannot be null or empty";
    public const string PseCodeCannotBeGreaterThan34Characters = "167 : Pse Code cannot be greater than 34 characters";
    public const string PseCodeMustBeValidFormat = "168 : Pse Code must be valid format";
    public const string TypePersonCannotBeNullOrEmpty = "169 : Type Person cannot be null or empty";
    public const string TypePersonCannotBeGreaterThan2Characters = "170 : Type Person cannot be greater than 2 characters";
    public const string PseResponseUrlCannotBeNullOrEmpty = "171 : Pse Response URL cannot be null or empty";
    public const string PseResponseUrlCannotBeGreaterThan255Characters = "172 : Pse Response URL cannot be greater than 255 characters";
    public const string PseCannotBeNull = "173 : Pse cannot be null";

    public const string BackDescriptionRequired = "174 : Back Description is required";

    public const string BackCodeRequired = "175 : Back Code is required";

    public const string BackNameRequired = "176 : Back Name is required";

    public const string IdPaymentMethodCannotBeEmpty = "177 : Id Payment Method cannot be empty";
    public const string NameOfPaymentMethodCannotBeNullOrEmpty = "178 : Name of Payment Method cannot be null or empty";
    public const string CodeOfPaymentMethodCannotBeNullOrEmpty = "179 : Code of Payment Method cannot be null or empty";
    public const string CodeOfPaymentMethodCannotBeGreaterThan32Characters = "180 : Code of Payment Method cannot be greater than 32 characters";

    public const string NameOfPaymentMethodCannotBeGreaterThan64Characters = "181 : Name of Payment Method cannot be greater than 64 characters";

    public const string CommentsOfPaymentMethodCannotBeGreaterThan124Characters = "182 : Comments of Payment Method cannot be greater than 124 characters";

    public const string ModuleCannotBeNullOrEmpty = "183 : Module cannot be null or empty";

    public const string PaymentMethodTypeCannotBeNullOrEmpty = "184 : Payment Method Type cannot be null or empty";
    public const string PaymentMethodTypeCannotBeGreaterThan50Characters = "185 : Payment Method Type cannot be greater than 50 characters";
    public const string PaymentMethodBrandCannotBeNullOrEmpty = "186 : Payment Method Brand cannot be null or empty";
    public const string PaymentMethodBrandCannotBeGreaterThan50Characters = "187 : Payment Method Brand cannot be greater than 50 characters";
    public const string PaymentMethodMaskedIdentifierCannotBeNullOrEmpty = "188 : Payment Method Masked Identifier cannot be null or empty";
    public const string PaymentMethodMaskedIdentifierCannotBeGreaterThan50Characters = "189 : Payment Method Masked Identifier cannot be greater than 50 characters";

    public const string SubTotalCannotBeNull = "190 : SubTotal cannot be null";
    public const string PaymentMethodCannotBeNull = "191 : Payment Method cannot be null";
    public const string PaymentProviderCannotBeNull = "192 : Payment Provider cannot be null";

    public const string TotalCannotBeNull = "193 : Total cannot be null";
    public const string PaymentStatusIsNotInitiated = "194 : Cannot complete a payment that is not in 'Initiated' state.";

    public const string ProviderTransactionIdCannotBeNullOrEmpty = "195 : Provider Transaction ID cannot be null or empty";
    public const string ProviderResponseMessageCannotBeNullOrEmpty = "196 : Provider Response Message cannot be null or empty";
    public const string RawProviderResponseDataCannotBeNullOrEmpty = "197 : Raw Provider Response Data cannot be null or empty";

    public const string TotalMustBeGreaterThanOrEqualToSubTotalPlusTax = "198 : Total must be greater than or equal to SubTotal plus Tax";

    public const string PaymentMethodInfoMustHaveOnePaymentMethod  = "199 : PaymentMethodInfo must have either CreditCard or Pse, but not both";

    public const string AmountValueMustBeGreaterThanZero  = "200 : Amount value must be greater than zero";

    public const string CreditCardInstallmentsNumberMustBeGreaterThanZero = "201 : Credit Card Installments Number must be greater than zero";

    public const string TransactionIdCannotBeNullOrEmpty  = "202 : Transaction ID cannot be null or empty";

    public const string FinancialNetworkCannotBeNull = "203 : Financial Network cannot be null";

    public const string CurrencyMustBeThreeLetterUppercaseISO4217Code = "204 : Currency must be a three-letter uppercase ISO 4217 code";
}
