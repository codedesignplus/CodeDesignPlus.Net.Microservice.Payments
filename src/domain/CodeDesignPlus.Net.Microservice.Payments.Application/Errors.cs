using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Payments.Application;

public class Errors: IErrorCodes
{    
    public static readonly Error UnknownError = new("200", "UnknownError");
    public static readonly Error InvalidRequest = new("201", "Invalid Request");
    public static readonly Error PaymentAlredyExists = new("202", "Payment already exists");
    public static readonly Error PaymentNotFound = new("203", "Payment not found");

    public static readonly Error PaymentMethodAlreadyExists = new("204", "Payment method already exists");

    public static readonly Error PaymentMethodNotFound = new("205", "Payment method not found");

    public static readonly Error PaymentProviderTransactionIdNotFound = new("206", "Payment provider transaction id not found");

    public static readonly Error InvalidSignature = new("207", "Invalid signature received");

    public static readonly Error PaymentProviderNotSupported = new("208", "Payment provider not supported");

    public static readonly Error NextActionInvalid = new("209", "Next action is invalid");

    // Beneficiary errors
    public static readonly Error BeneficiaryAlreadyExists = new("210", "Beneficiary already exists");
    public static readonly Error BeneficiaryNotFound = new("211", "Beneficiary not found");

    // DisbursementRule errors
    public static readonly Error DisbursementRuleAlreadyExists = new("212", "Disbursement Rule already exists");
    public static readonly Error DisbursementRuleNotFound = new("213", "Disbursement Rule not found");

    // Disbursement errors
    public static readonly Error DisbursementAlreadyExists = new("214", "Disbursement already exists");
    public static readonly Error DisbursementNotFound = new("215", "Disbursement not found");

    // SavedCard errors
    public static readonly Error SavedCardAlreadyExists = new("216", "Saved Card already exists");
    public static readonly Error SavedCardNotFound = new("217", "Saved Card not found");

    public static readonly Error TokenizationFailed = new("218", "Tokenization failed");

    public static readonly Error PaymentProviderConfigAlreadyExists = new("219", "Payment provider config already exists");
    public static readonly Error PaymentProviderConfigNotFound = new("220", "Payment provider config not found");
}
