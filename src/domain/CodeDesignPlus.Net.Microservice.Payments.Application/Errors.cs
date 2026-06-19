namespace CodeDesignPlus.Net.Microservice.Payments.Application;

public class Errors: IErrorCodes
{    
    public const string UnknownError = "200 : UnknownError";
    public const string InvalidRequest = "201 : Invalid Request";
    public const string PaymentAlredyExists = "202 : Payment already exists";
    public const string PaymentNotFound = "203 : Payment not found";

    public const string PaymentMethodAlreadyExists = "204 : Payment method already exists";

    public const string PaymentMethodNotFound = "205 : Payment method not found";

    public const string PaymentProviderTransactionIdNotFound = "206 : Payment provider transaction id not found";

    public const string InvalidSignature = "207 : Invalid signature received";

    public const string PaymentProviderNotSupported = "208 : Payment provider not supported";

    public const string NextActionInvalid = "209 : Next action is invalid";

    // Beneficiary errors
    public const string BeneficiaryAlreadyExists = "210 : Beneficiary already exists";
    public const string BeneficiaryNotFound = "211 : Beneficiary not found";

    // DisbursementRule errors
    public const string DisbursementRuleAlreadyExists = "212 : Disbursement Rule already exists";
    public const string DisbursementRuleNotFound = "213 : Disbursement Rule not found";

    // Disbursement errors
    public const string DisbursementAlreadyExists = "214 : Disbursement already exists";
    public const string DisbursementNotFound = "215 : Disbursement not found";

    // SavedCard errors
    public const string SavedCardAlreadyExists = "216 : Saved Card already exists";
    public const string SavedCardNotFound = "217 : Saved Card not found";

    public const string TokenizationFailed = "218 : Tokenization failed";

    public const string PaymentProviderConfigAlreadyExists = "219 : Payment provider config already exists";
    public const string PaymentProviderConfigNotFound = "220 : Payment provider config not found";
}
