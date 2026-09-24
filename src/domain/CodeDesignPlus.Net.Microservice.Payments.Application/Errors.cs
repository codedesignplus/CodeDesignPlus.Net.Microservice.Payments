using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Payments.Application;

public class Errors: IErrorCodes
{    
    public static readonly Error UnknownError = new("200");
    public static readonly Error InvalidRequest = new("201");
    public static readonly Error PaymentAlredyExists = new("202");
    public static readonly Error PaymentNotFound = new("203");

    public static readonly Error PaymentMethodAlreadyExists = new("204");

    public static readonly Error PaymentMethodNotFound = new("205");

    public static readonly Error PaymentProviderTransactionIdNotFound = new("206");

    public static readonly Error InvalidSignature = new("207");

    public static readonly Error PaymentProviderNotSupported = new("208");

    public static readonly Error NextActionInvalid = new("209");

    // Beneficiary errors
    public static readonly Error BeneficiaryAlreadyExists = new("210");
    public static readonly Error BeneficiaryNotFound = new("211");

    // DisbursementRule errors
    public static readonly Error DisbursementRuleAlreadyExists = new("212");
    public static readonly Error DisbursementRuleNotFound = new("213");

    // Disbursement errors
    public static readonly Error DisbursementAlreadyExists = new("214");
    public static readonly Error DisbursementNotFound = new("215");

    // SavedCard errors
    public static readonly Error SavedCardAlreadyExists = new("216");
    public static readonly Error SavedCardNotFound = new("217");

    public static readonly Error TokenizationFailed = new("218");

    public static readonly Error PaymentProviderConfigAlreadyExists = new("219");
    public static readonly Error PaymentProviderConfigNotFound = new("220");
}
