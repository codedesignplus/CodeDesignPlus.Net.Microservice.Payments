using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class Errors : IErrorCodes
{
    public static readonly Error UnknownError = new("100");
    public static readonly Error DescriptionCannotBeNullOrEmpty = new("101");
    public static readonly Error DescriptionCannotBeGreaterThan255Characters = new("102");
    public static readonly Error BuyerCannotBeNull = new("103");
    public static readonly Error StreetCannotBeNullOrEmpty = new("104");
    public static readonly Error StreetCannotBeGreaterThan100Characters = new("105");
    public static readonly Error CountryCannotBeNullOrEmpty = new("106");
    public static readonly Error CountryCannotBeGreaterThan2Characters = new("107");
    public static readonly Error CountryMustBeTwoUppercaseLetters = new("108");
    public static readonly Error StateCannotBeNullOrEmpty = new("109");
    public static readonly Error StateCannotBeGreaterThan40Characters = new("110");
    public static readonly Error CityCannotBeNullOrEmpty = new("111");
    public static readonly Error CityCannotBeGreaterThan50Characters = new("112");
    public static readonly Error PostalCodeCannotBeNullOrEmpty = new("113");
    public static readonly Error PostalCodeCannotBeGreaterThan8Characters = new("114");
    public static readonly Error PostalCodeMustBeValidFormat = new("115");
    public static readonly Error PhoneCannotBeNullOrEmpty = new("116");
    public static readonly Error PhoneCannotBeGreaterThan11Characters = new("117");
    public static readonly Error PhoneMustBeValidFormat = new("118");
    public static readonly Error FullNameCannotBeNullOrEmpty = new("119");
    public static readonly Error FullNameCannotBeGreaterThan100Characters = new("120");
    public static readonly Error EmailAddressCannotBeNullOrEmpty = new("121");
    public static readonly Error EmailAddressCannotBeGreaterThan255Characters = new("122");
    public static readonly Error EmailAddressMustBeValidFormat = new("123");
    public static readonly Error ContactPhoneCannotBeNullOrEmpty = new("124");
    public static readonly Error ContactPhoneCannotBeGreaterThan20Characters = new("125");
    public static readonly Error ShippingAddressCannotBeNullOrEmpty = new("126");
    public static readonly Error DniNumberCannotBeNullOrEmpty = new("127");
    public static readonly Error DniNumberCannotBeGreaterThan20Characters = new("128");
    public static readonly Error FullNameCannotBeGreaterThan150Characters = new("129");
    public static readonly Error BillingAddressCannotBeNull = new("130");
    public static readonly Error OrderCannotBeNull = new("131");
    public static readonly Error PaymentMethodCannotBeNullOrEmpty = new("132");
    public static readonly Error PaymentMethodCannotBeGreaterThan32Characters = new("133");
    public static readonly Error CreditCardCannotBeNull = new("134");

    public static readonly Error ProviderCannotBeNullOrEmpty = new("135");
    public static readonly Error ProviderCannotBeGreaterThan50Characters = new("136");
    public static readonly Error TransactionCannotBeNull = new("137");
    public static readonly Error RequestCannotBeNull = new("138");
    public static readonly Error ResponseCannotBeNull = new("139");

    public static readonly Error CurrencyCannotBeNullOrEmpty = new("140");
    public static readonly Error CurrencyMustBeValidFormat = new("141");

    public static readonly Error DeviceSessionIdMustBeValidFormat = new("142");
    public static readonly Error DeviceSessionIdCannotBeNullOrEmpty = new("143");

    public static readonly Error CreditCardNumberCannotBeNullOrEmpty = new("144");
    public static readonly Error CreditCardNumberCannotBeGreaterThan20Characters = new("145");
    public static readonly Error CreditCardNumberCannotBeLessThan13Characters = new("146");
    public static readonly Error CreditCardSecurityCodeCannotBeNullOrEmpty = new("147");
    public static readonly Error CreditCardSecurityCodeCannotBeGreaterThan4Characters = new("148");
    public static readonly Error CreditCardSecurityCodeCannotBeLessThan3Characters = new("149");
    public static readonly Error CreditCardExpirationDateCannotBeNullOrEmpty = new("150");
    public static readonly Error CreditCardExpirationDateMustBeValidFormat = new("151");

    public static readonly Error CreditCardExpirationDateCannotBeGreaterThan7Characters = new("152");

    public static readonly Error DeviceSessionIdCannotBeGreaterThan255Characters = new("153");
    public static readonly Error IpAddressCannotBeNullOrEmpty = new("154");
    public static readonly Error IpAddressCannotBeGreaterThan39Characters = new("155");
    public static readonly Error IpAddressMustBeValidFormat = new("156");
    public static readonly Error CookieCannotBeNullOrEmpty = new("157");
    public static readonly Error CookieCannotBeGreaterThan255Characters = new("158");
    public static readonly Error UserAgentCannotBeNullOrEmpty = new("159");
    public static readonly Error UserAgentCannotBeGreaterThan1024Characters = new("160");

    public static readonly Error AmountCannotBeNull = new("161");
    public static readonly Error TaxCannotBeNull = new("162");
    public static readonly Error TaxReturnBaseCannotBeNull = new("163");

    public static readonly Error DniTypeCannotBeNullOrEmpty = new("164");
    public static readonly Error DniTypeCannotBeGreaterThan3Characters = new("165");

    public static readonly Error PseCodeCannotBeNullOrEmpty = new("166");
    public static readonly Error PseCodeCannotBeGreaterThan34Characters = new("167");
    public static readonly Error PseCodeMustBeValidFormat = new("168");
    public static readonly Error TypePersonCannotBeNullOrEmpty = new("169");
    public static readonly Error TypePersonCannotBeGreaterThan2Characters = new("170");
    public static readonly Error PseResponseUrlCannotBeNullOrEmpty = new("171");
    public static readonly Error PseResponseUrlCannotBeGreaterThan255Characters = new("172");
    public static readonly Error PseCannotBeNull = new("173");

    public static readonly Error BackDescriptionRequired = new("174");

    public static readonly Error BackCodeRequired = new("175");

    public static readonly Error BackNameRequired = new("176");

    public static readonly Error IdPaymentMethodCannotBeEmpty = new("177");
    public static readonly Error NameOfPaymentMethodCannotBeNullOrEmpty = new("178");
    public static readonly Error CodeOfPaymentMethodCannotBeNullOrEmpty = new("179");
    public static readonly Error CodeOfPaymentMethodCannotBeGreaterThan32Characters = new("180");

    public static readonly Error NameOfPaymentMethodCannotBeGreaterThan64Characters = new("181");

    public static readonly Error CommentsOfPaymentMethodCannotBeGreaterThan124Characters = new("182");

    public static readonly Error ModuleCannotBeNullOrEmpty = new("183");

    public static readonly Error PaymentMethodTypeCannotBeNullOrEmpty = new("184");
    public static readonly Error PaymentMethodTypeCannotBeGreaterThan50Characters = new("185");
    public static readonly Error PaymentMethodBrandCannotBeNullOrEmpty = new("186");
    public static readonly Error PaymentMethodBrandCannotBeGreaterThan50Characters = new("187");
    public static readonly Error PaymentMethodMaskedIdentifierCannotBeNullOrEmpty = new("188");
    public static readonly Error PaymentMethodMaskedIdentifierCannotBeGreaterThan50Characters = new("189");

    public static readonly Error SubTotalCannotBeNull = new("190");
    public static readonly Error PaymentMethodCannotBeNull = new("191");
    public static readonly Error PaymentProviderCannotBeNull = new("192");

    public static readonly Error TotalCannotBeNull = new("193");
    public static readonly Error PaymentStatusIsNotInitiated = new("194");

    public static readonly Error ProviderTransactionIdCannotBeNullOrEmpty = new("195");
    public static readonly Error ProviderResponseMessageCannotBeNullOrEmpty = new("196");
    public static readonly Error RawProviderResponseDataCannotBeNullOrEmpty = new("197");

    public static readonly Error TotalMustBeGreaterThanOrEqualToSubTotalPlusTax = new("198");

    public static readonly Error PaymentMethodInfoMustHaveOnePaymentMethod = new("199");

    public static readonly Error AmountValueMustBeGreaterThanZero = new("264");

    public static readonly Error CreditCardInstallmentsNumberMustBeGreaterThanZero = new("265");

    public static readonly Error TransactionIdCannotBeNullOrEmpty = new("266");

    public static readonly Error FinancialNetworkCannotBeNull = new("267");

    public static readonly Error CurrencyMustBeThreeLetterUppercaseISO4217Code = new("268");

    public static readonly Error ResponseCannotBeNullOrEmpty = new("269");

    public static readonly Error OnlyCanSetInitiateResponseIfStatusIsInitiated = new("270");
    public static readonly Error InitiateResponseCannotBeEmpty = new("271");

    public static readonly Error OnlyCanSetFinalResponseIfStatusIsInitiated = new("272");
    public static readonly Error FinalStatusMustBeResolved = new("273");
    public static readonly Error FinalResponseCannotBeEmpty = new("274");

    public static readonly Error ReferenceIdCannotBeEmpty = new("275");

    public static readonly Error PaymentIdCannotBeEmpty = new("276");

    public static readonly Error PayerIdCannotBeEmpty = new("277");
    public static readonly Error CreditCardTokenIdCannotBeNullOrEmpty = new("278");
    public static readonly Error CreditCardNameCannotBeNullOrEmpty = new("279");
    public static readonly Error CreditCardIdentificationNumberCannotBeNullOrEmpty = new("280");
    public static readonly Error CreditCardPaymentMethodCannotBeNullOrEmpty = new("281");
    public static readonly Error CreditCardMaskedNumberCannotBeNullOrEmpty = new("282");

    public static readonly Error TotalMustBeEqualToSubTotalPlusTax = new("283");

    public static readonly Error PaymentProviderIsRequired = new("284");

    public static readonly Error PaymentMethodMustHaveExactlyOneOption = new("221");

    public static readonly Error TypePersonCannotBeGreaterThan1Character = new("222");
    public static readonly Error PseResponseUrlMustBeValidFormat = new("223");

    public static readonly Error CreditCardLast4DigitsCannotBeNullOrEmpty = new("224");
    public static readonly Error CreditCardCardHolderNameCannotBeNullOrEmpty = new("225");

    public static readonly Error CurrencyIsRequired = new("226");

    // Beneficiary errors
    public static readonly Error BeneficiaryUserIdIsRequired = new("227");
    public static readonly Error BankCodeIsRequired = new("228");
    public static readonly Error AccountNumberIsRequired = new("229");
    public static readonly Error DocumentNumberIsRequired = new("230");
    public static readonly Error HolderNameIsRequired = new("231");
    public static readonly Error DocumentTypeIsRequired = new("232");
    public static readonly Error BankNameIsRequired = new("233");
    public static readonly Error CountryIsRequired = new("234");

    // DisbursementRule errors
    public static readonly Error CommissionAmountMustBePositive = new("235");
    public static readonly Error DisbursementRuleCurrencyIsRequired = new("236");

    // Disbursement errors
    public static readonly Error DisbursementPaymentIdIsRequired = new("237");
    public static readonly Error DisbursementBeneficiaryIdIsRequired = new("238");
    public static readonly Error DisbursementBeneficiaryUserIdIsRequired = new("239");
    public static readonly Error DisbursementAmountMustBePositive = new("240");
    public static readonly Error DisbursementTotalAmountMustBePositive = new("241");
    public static readonly Error DisbursementNotPending = new("242");
    public static readonly Error DisbursementNotProcessing = new("243");
    public static readonly Error ProviderReferenceIsRequired = new("244");
    public static readonly Error FailureReasonIsRequired = new("245");

    // SavedCard errors
    public static readonly Error SavedCardIdCannotBeEmpty = new("246");
    public static readonly Error SavedCardUserIdCannotBeEmpty = new("247");
    public static readonly Error SavedCardTokenCannotBeNullOrEmpty = new("248");
    public static readonly Error SavedCardMaskedNumberCannotBeNullOrEmpty = new("249");
    public static readonly Error SavedCardFranchiseCannotBeNullOrEmpty = new("250");
    public static readonly Error SavedCardCardHolderNameCannotBeNullOrEmpty = new("251");
    public static readonly Error SavedCardExpirationDateCannotBeNullOrEmpty = new("252");
    public static readonly Error SavedCardLast4DigitsCannotBeNullOrEmpty = new("253");

    // PaymentProviderConfig errors
    public static readonly Error PaymentProviderConfigIdIsRequired = new("254");
    public static readonly Error PaymentProviderConfigMerchantIdIsRequired = new("255");
    public static readonly Error PaymentProviderConfigApiKeyIsRequired = new("256");
    public static readonly Error PaymentProviderConfigApiLoginIsRequired = new("257");
    public static readonly Error PaymentProviderConfigSecretKeyIsRequired = new("258");
    public static readonly Error PaymentProviderConfigNotificationUrlIsRequired = new("259"); // reserved

    public static readonly Error CommissionBasisPointsMustBePositive = new("260");
    public static readonly Error PaymentProviderConfigNotFound = new("261");
    public static readonly Error PaymentProviderConfigAlreadyExists = new("262");

    public static readonly Error OnlyPaymentsInProgressCanExpire = new("263");
}
