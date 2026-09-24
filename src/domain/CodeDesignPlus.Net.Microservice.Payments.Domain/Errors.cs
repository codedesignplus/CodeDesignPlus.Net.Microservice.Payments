using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class Errors : IErrorCodes
{
    public static readonly Error UnknownError = new("100", "UnknownError");
    public static readonly Error DescriptionCannotBeNullOrEmpty = new("101", "Description cannot be null or empty");
    public static readonly Error DescriptionCannotBeGreaterThan255Characters = new("102", "Description cannot be greater than 255 characters");
    public static readonly Error BuyerCannotBeNull = new("103", "Buyer cannot be null");
    public static readonly Error StreetCannotBeNullOrEmpty = new("104", "Street cannot be null or empty");
    public static readonly Error StreetCannotBeGreaterThan100Characters = new("105", "Street cannot be greater than 100 characters");
    public static readonly Error CountryCannotBeNullOrEmpty = new("106", "Country cannot be null or empty");
    public static readonly Error CountryCannotBeGreaterThan2Characters = new("107", "Country cannot be greater than 2 characters");
    public static readonly Error CountryMustBeTwoUppercaseLetters = new("108", "Country must be two uppercase letters");
    public static readonly Error StateCannotBeNullOrEmpty = new("109", "State cannot be null or empty");
    public static readonly Error StateCannotBeGreaterThan40Characters = new("110", "State cannot be greater than 40 characters");
    public static readonly Error CityCannotBeNullOrEmpty = new("111", "City cannot be null or empty");
    public static readonly Error CityCannotBeGreaterThan50Characters = new("112", "City cannot be greater than 50 characters");
    public static readonly Error PostalCodeCannotBeNullOrEmpty = new("113", "PostalCode cannot be null or empty");
    public static readonly Error PostalCodeCannotBeGreaterThan8Characters = new("114", "PostalCode cannot be greater than 8 characters");
    public static readonly Error PostalCodeMustBeValidFormat = new("115", "PostalCode must be valid format");
    public static readonly Error PhoneCannotBeNullOrEmpty = new("116", "Phone cannot be null or empty");
    public static readonly Error PhoneCannotBeGreaterThan11Characters = new("117", "Phone cannot be greater than 11 characters");
    public static readonly Error PhoneMustBeValidFormat = new("118", "Phone must be valid format");
    public static readonly Error FullNameCannotBeNullOrEmpty = new("119", "FullName cannot be null or empty");
    public static readonly Error FullNameCannotBeGreaterThan100Characters = new("120", "FullName cannot be greater than 100 characters");
    public static readonly Error EmailAddressCannotBeNullOrEmpty = new("121", "EmailAddress cannot be null or empty");
    public static readonly Error EmailAddressCannotBeGreaterThan255Characters = new("122", "EmailAddress cannot be greater than 255 characters");
    public static readonly Error EmailAddressMustBeValidFormat = new("123", "EmailAddress must be valid format");
    public static readonly Error ContactPhoneCannotBeNullOrEmpty = new("124", "ContactPhone cannot be null or empty");
    public static readonly Error ContactPhoneCannotBeGreaterThan20Characters = new("125", "ContactPhone cannot be greater than 20 characters");
    public static readonly Error ShippingAddressCannotBeNullOrEmpty = new("126", "Shipping Address cannot be null or empty");
    public static readonly Error DniNumberCannotBeNullOrEmpty = new("127", "DniNumber cannot be null or empty");
    public static readonly Error DniNumberCannotBeGreaterThan20Characters = new("128", "DniNumber cannot be greater than 20 characters");
    public static readonly Error FullNameCannotBeGreaterThan150Characters = new("129", "FullName cannot be greater than 150 characters");
    public static readonly Error BillingAddressCannotBeNull = new("130", "BillingAddress cannot be null");
    public static readonly Error OrderCannotBeNull = new("131", "Order cannot be null");
    public static readonly Error PaymentMethodCannotBeNullOrEmpty = new("132", "PaymentMethod cannot be null or empty");
    public static readonly Error PaymentMethodCannotBeGreaterThan32Characters = new("133", "PaymentMethod cannot be greater than 32 characters");
    public static readonly Error CreditCardCannotBeNull = new("134", "CreditCard cannot be null");

    public static readonly Error ProviderCannotBeNullOrEmpty = new("135", "Provider cannot be null or empty");
    public static readonly Error ProviderCannotBeGreaterThan50Characters = new("136", "Provider cannot be greater than 50 characters");
    public static readonly Error TransactionCannotBeNull = new("137", "Transaction cannot be null");
    public static readonly Error RequestCannotBeNull = new("138", "Request cannot be null");
    public static readonly Error ResponseCannotBeNull = new("139", "Response cannot be null");

    public static readonly Error CurrencyCannotBeNullOrEmpty = new("140", "Currency cannot be null or empty");
    public static readonly Error CurrencyMustBeValidFormat = new("141", "Currency must be valid format");

    public static readonly Error DeviceSessionIdMustBeValidFormat = new("142", "Device SessionId must be valid format");
    public static readonly Error DeviceSessionIdCannotBeNullOrEmpty = new("143", "Device SessionId cannot be null or empty");

    public static readonly Error CreditCardNumberCannotBeNullOrEmpty = new("144", "Credit Card Number cannot be null or empty");
    public static readonly Error CreditCardNumberCannotBeGreaterThan20Characters = new("145", "Credit Card Number cannot be greater than 20 characters");
    public static readonly Error CreditCardNumberCannotBeLessThan13Characters = new("146", "Credit Card Number cannot be less than 13 characters");
    public static readonly Error CreditCardSecurityCodeCannotBeNullOrEmpty = new("147", "Credit Card Security Code cannot be null or empty");
    public static readonly Error CreditCardSecurityCodeCannotBeGreaterThan4Characters = new("148", "Credit Card Security Code cannot be greater than 4 characters");
    public static readonly Error CreditCardSecurityCodeCannotBeLessThan3Characters = new("149", "Credit Card Security Code cannot be less than 3 characters");
    public static readonly Error CreditCardExpirationDateCannotBeNullOrEmpty = new("150", "Credit Card Expiration Date cannot be null or empty");
    public static readonly Error CreditCardExpirationDateMustBeValidFormat = new("151", "Credit Card Expiration Date must be valid format");

    public static readonly Error CreditCardExpirationDateCannotBeGreaterThan7Characters = new("152", "Credit Card Expiration Date cannot be greater than 7 characters");

    public static readonly Error DeviceSessionIdCannotBeGreaterThan255Characters = new("153", "Device SessionId cannot be greater than 255 characters");
    public static readonly Error IpAddressCannotBeNullOrEmpty = new("154", "IP Address cannot be null or empty");
    public static readonly Error IpAddressCannotBeGreaterThan39Characters = new("155", "IP Address cannot be greater than 39 characters");
    public static readonly Error IpAddressMustBeValidFormat = new("156", "IP Address must be valid format");
    public static readonly Error CookieCannotBeNullOrEmpty = new("157", "Cookie cannot be null or empty");
    public static readonly Error CookieCannotBeGreaterThan255Characters = new("158", "Cookie cannot be greater than 255 characters");
    public static readonly Error UserAgentCannotBeNullOrEmpty = new("159", "User Agent cannot be null or empty");
    public static readonly Error UserAgentCannotBeGreaterThan1024Characters = new("160", "User Agent cannot be greater than 1024 characters");

    public static readonly Error AmountCannotBeNull = new("161", "Amount cannot be null");
    public static readonly Error TaxCannotBeNull = new("162", "Tax cannot be null");
    public static readonly Error TaxReturnBaseCannotBeNull = new("163", "Tax Return Base cannot be null");

    public static readonly Error DniTypeCannotBeNullOrEmpty = new("164", "Dni Type cannot be null or empty");
    public static readonly Error DniTypeCannotBeGreaterThan3Characters = new("165", "Dni Type cannot be greater than 3 characters");

    public static readonly Error PseCodeCannotBeNullOrEmpty = new("166", "Pse Code cannot be null or empty");
    public static readonly Error PseCodeCannotBeGreaterThan34Characters = new("167", "Pse Code cannot be greater than 34 characters");
    public static readonly Error PseCodeMustBeValidFormat = new("168", "Pse Code must be valid format");
    public static readonly Error TypePersonCannotBeNullOrEmpty = new("169", "Type Person cannot be null or empty");
    public static readonly Error TypePersonCannotBeGreaterThan2Characters = new("170", "Type Person cannot be greater than 2 characters");
    public static readonly Error PseResponseUrlCannotBeNullOrEmpty = new("171", "Pse Response URL cannot be null or empty");
    public static readonly Error PseResponseUrlCannotBeGreaterThan255Characters = new("172", "Pse Response URL cannot be greater than 255 characters");
    public static readonly Error PseCannotBeNull = new("173", "Pse cannot be null");

    public static readonly Error BackDescriptionRequired = new("174", "Back Description is required");

    public static readonly Error BackCodeRequired = new("175", "Back Code is required");

    public static readonly Error BackNameRequired = new("176", "Back Name is required");

    public static readonly Error IdPaymentMethodCannotBeEmpty = new("177", "Id Payment Method cannot be empty");
    public static readonly Error NameOfPaymentMethodCannotBeNullOrEmpty = new("178", "Name of Payment Method cannot be null or empty");
    public static readonly Error CodeOfPaymentMethodCannotBeNullOrEmpty = new("179", "Code of Payment Method cannot be null or empty");
    public static readonly Error CodeOfPaymentMethodCannotBeGreaterThan32Characters = new("180", "Code of Payment Method cannot be greater than 32 characters");

    public static readonly Error NameOfPaymentMethodCannotBeGreaterThan64Characters = new("181", "Name of Payment Method cannot be greater than 64 characters");

    public static readonly Error CommentsOfPaymentMethodCannotBeGreaterThan124Characters = new("182", "Comments of Payment Method cannot be greater than 124 characters");

    public static readonly Error ModuleCannotBeNullOrEmpty = new("183", "Module cannot be null or empty");

    public static readonly Error PaymentMethodTypeCannotBeNullOrEmpty = new("184", "Payment Method Type cannot be null or empty");
    public static readonly Error PaymentMethodTypeCannotBeGreaterThan50Characters = new("185", "Payment Method Type cannot be greater than 50 characters");
    public static readonly Error PaymentMethodBrandCannotBeNullOrEmpty = new("186", "Payment Method Brand cannot be null or empty");
    public static readonly Error PaymentMethodBrandCannotBeGreaterThan50Characters = new("187", "Payment Method Brand cannot be greater than 50 characters");
    public static readonly Error PaymentMethodMaskedIdentifierCannotBeNullOrEmpty = new("188", "Payment Method Masked Identifier cannot be null or empty");
    public static readonly Error PaymentMethodMaskedIdentifierCannotBeGreaterThan50Characters = new("189", "Payment Method Masked Identifier cannot be greater than 50 characters");

    public static readonly Error SubTotalCannotBeNull = new("190", "SubTotal cannot be null");
    public static readonly Error PaymentMethodCannotBeNull = new("191", "Payment Method cannot be null");
    public static readonly Error PaymentProviderCannotBeNull = new("192", "Payment Provider cannot be null");

    public static readonly Error TotalCannotBeNull = new("193", "Total cannot be null");
    public static readonly Error PaymentStatusIsNotInitiated = new("194", "Cannot complete a payment that is not in 'Initiated' state.");

    public static readonly Error ProviderTransactionIdCannotBeNullOrEmpty = new("195", "Provider Transaction ID cannot be null or empty");
    public static readonly Error ProviderResponseMessageCannotBeNullOrEmpty = new("196", "Provider Response Message cannot be null or empty");
    public static readonly Error RawProviderResponseDataCannotBeNullOrEmpty = new("197", "Raw Provider Response Data cannot be null or empty");

    public static readonly Error TotalMustBeGreaterThanOrEqualToSubTotalPlusTax = new("198", "Total must be greater than or equal to SubTotal plus Tax");

    public static readonly Error PaymentMethodInfoMustHaveOnePaymentMethod = new("199", "PaymentMethodInfo must have either CreditCard or Pse, but not both");

    public static readonly Error AmountValueMustBeGreaterThanZero = new("264", "Amount value must be greater than zero");

    public static readonly Error CreditCardInstallmentsNumberMustBeGreaterThanZero = new("265", "Credit Card Installments Number must be greater than zero");

    public static readonly Error TransactionIdCannotBeNullOrEmpty = new("266", "Transaction ID cannot be null or empty");

    public static readonly Error FinancialNetworkCannotBeNull = new("267", "Financial Network cannot be null");

    public static readonly Error CurrencyMustBeThreeLetterUppercaseISO4217Code = new("268", "Currency must be a three-letter uppercase ISO 4217 code");

    public static readonly Error ResponseCannotBeNullOrEmpty = new("269", "Response cannot be null or empty");

    public static readonly Error OnlyCanSetInitiateResponseIfStatusIsInitiated = new("270", "Only can set initiate response if status is 'Initiated'");
    public static readonly Error InitiateResponseCannotBeEmpty = new("271", "Initiate response cannot be empty");

    public static readonly Error OnlyCanSetFinalResponseIfStatusIsInitiated = new("272", "Only can set final response if status is 'Initiated'");
    public static readonly Error FinalStatusMustBeResolved = new("273", "Final status must be 'Succeeded', 'Failed' or 'Expired'");
    public static readonly Error FinalResponseCannotBeEmpty = new("274", "Final response cannot be empty");

    public static readonly Error ReferenceIdCannotBeEmpty = new("275", "Reference ID cannot be empty");

    public static readonly Error PaymentIdCannotBeEmpty = new("276", "Payment ID cannot be empty");

    public static readonly Error PayerIdCannotBeEmpty = new("277", "Payer ID cannot be empty");
    public static readonly Error CreditCardTokenIdCannotBeNullOrEmpty = new("278", "Credit Card Token ID cannot be null or empty");
    public static readonly Error CreditCardNameCannotBeNullOrEmpty = new("279", "Credit Card Name cannot be null or empty");
    public static readonly Error CreditCardIdentificationNumberCannotBeNullOrEmpty = new("280", "Credit Card Identification Number cannot be null or empty");
    public static readonly Error CreditCardPaymentMethodCannotBeNullOrEmpty = new("281", "Credit Card Payment Method cannot be null or empty");
    public static readonly Error CreditCardMaskedNumberCannotBeNullOrEmpty = new("282", "Credit Card Masked Number cannot be null or empty");

    public static readonly Error TotalMustBeEqualToSubTotalPlusTax = new("283", "Total must be equal to SubTotal plus Tax");

    public static readonly Error PaymentProviderIsRequired = new("284", "Payment Provider is required");

    public static readonly Error PaymentMethodMustHaveExactlyOneOption = new("221", "PaymentMethodInfo must have either CreditCard or Pse, but not both");

    public static readonly Error TypePersonCannotBeGreaterThan1Character = new("222", "TypePerson cannot be greater than 1 character");
    public static readonly Error PseResponseUrlMustBeValidFormat = new("223", "Pse Response URL must be a valid format");

    public static readonly Error CreditCardLast4DigitsCannotBeNullOrEmpty = new("224", "Credit Card Last 4 Digits cannot be null or empty");
    public static readonly Error CreditCardCardHolderNameCannotBeNullOrEmpty = new("225", "Credit Card Card Holder Name cannot be null or empty");

    public static readonly Error CurrencyIsRequired = new("226", "Currency is required");

    // Beneficiary errors
    public static readonly Error BeneficiaryUserIdIsRequired = new("227", "Beneficiary UserId is required");
    public static readonly Error BankCodeIsRequired = new("228", "Bank Code is required");
    public static readonly Error AccountNumberIsRequired = new("229", "Account Number is required");
    public static readonly Error DocumentNumberIsRequired = new("230", "Document Number is required");
    public static readonly Error HolderNameIsRequired = new("231", "Holder Name is required");
    public static readonly Error DocumentTypeIsRequired = new("232", "Document Type is required");
    public static readonly Error BankNameIsRequired = new("233", "Bank Name is required");
    public static readonly Error CountryIsRequired = new("234", "Country is required");

    // DisbursementRule errors
    public static readonly Error CommissionAmountMustBePositive = new("235", "Commission Amount must be positive");
    public static readonly Error DisbursementRuleCurrencyIsRequired = new("236", "Disbursement Rule Currency is required");

    // Disbursement errors
    public static readonly Error DisbursementPaymentIdIsRequired = new("237", "Disbursement Payment ID is required");
    public static readonly Error DisbursementBeneficiaryIdIsRequired = new("238", "Disbursement Beneficiary ID is required");
    public static readonly Error DisbursementBeneficiaryUserIdIsRequired = new("239", "Disbursement Beneficiary User ID is required");
    public static readonly Error DisbursementAmountMustBePositive = new("240", "Disbursement Amount must be positive");
    public static readonly Error DisbursementTotalAmountMustBePositive = new("241", "Disbursement Total Amount must be positive");
    public static readonly Error DisbursementNotPending = new("242", "Disbursement is not in Pending status");
    public static readonly Error DisbursementNotProcessing = new("243", "Disbursement is not in Processing status");
    public static readonly Error ProviderReferenceIsRequired = new("244", "Provider Reference is required");
    public static readonly Error FailureReasonIsRequired = new("245", "Failure Reason is required");

    // SavedCard errors
    public static readonly Error SavedCardIdCannotBeEmpty = new("246", "Saved Card ID cannot be empty");
    public static readonly Error SavedCardUserIdCannotBeEmpty = new("247", "Saved Card User ID cannot be empty");
    public static readonly Error SavedCardTokenCannotBeNullOrEmpty = new("248", "Saved Card Token cannot be null or empty");
    public static readonly Error SavedCardMaskedNumberCannotBeNullOrEmpty = new("249", "Saved Card Masked Number cannot be null or empty");
    public static readonly Error SavedCardFranchiseCannotBeNullOrEmpty = new("250", "Saved Card Franchise cannot be null or empty");
    public static readonly Error SavedCardCardHolderNameCannotBeNullOrEmpty = new("251", "Saved Card Card Holder Name cannot be null or empty");
    public static readonly Error SavedCardExpirationDateCannotBeNullOrEmpty = new("252", "Saved Card Expiration Date cannot be null or empty");
    public static readonly Error SavedCardLast4DigitsCannotBeNullOrEmpty = new("253", "Saved Card Last 4 Digits cannot be null or empty");

    // PaymentProviderConfig errors
    public static readonly Error PaymentProviderConfigIdIsRequired = new("254", "Payment Provider Config ID is required");
    public static readonly Error PaymentProviderConfigMerchantIdIsRequired = new("255", "Merchant ID is required");
    public static readonly Error PaymentProviderConfigApiKeyIsRequired = new("256", "API Key is required");
    public static readonly Error PaymentProviderConfigApiLoginIsRequired = new("257", "API Login is required");
    public static readonly Error PaymentProviderConfigSecretKeyIsRequired = new("258", "Secret Key is required");
    public static readonly Error PaymentProviderConfigNotificationUrlIsRequired = new("259", "Notification URL is required"); // reserved

    public static readonly Error CommissionBasisPointsMustBePositive = new("260", "Commission Basis Points must be positive");
    public static readonly Error PaymentProviderConfigNotFound = new("261", "Payment Provider Config not found");
    public static readonly Error PaymentProviderConfigAlreadyExists = new("262", "Payment Provider Config already exists");

    public static readonly Error OnlyPaymentsInProgressCanExpire = new("263", "Only a payment still in progress can expire");
}
