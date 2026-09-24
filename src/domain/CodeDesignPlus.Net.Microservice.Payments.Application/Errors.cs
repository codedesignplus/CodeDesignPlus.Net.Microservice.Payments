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

    /// <summary>La lista de bancos es obligatoria.</summary>
    public static readonly Error BanksListIsRequired = new("285");

    /// <summary>La lista de bancos no puede traer entradas vacías.</summary>
    public static readonly Error BanksListHasEmptyEntries = new("286");

    /// <summary>Indica los datos de la tarjeta o los de PSE, pero no los dos.</summary>
    public static readonly Error PaymentMethodIsAmbiguous = new("287");

    /// <summary>La moneda debe ser un código ISO 4217 de tres letras en mayúscula.</summary>
    public static readonly Error CurrencyFormatIsInvalid = new("288");

    /// <summary>El país debe ser un código ISO 3166-1 alfa-2 de dos letras en mayúscula.</summary>
    public static readonly Error CountryFormatIsInvalid = new("289");

    /// <summary>La fecha de vencimiento debe seguir el formato AAAA/MM.</summary>
    public static readonly Error ExpirationDateFormatIsInvalid = new("290");

    /// <summary>La URL no es válida.</summary>
    public static readonly Error UrlIsInvalid = new("291");

    /// <summary>Los últimos cuatro dígitos deben ser exactamente cuatro números.</summary>
    public static readonly Error LastFourDigitsFormatIsInvalid = new("292");

    /// <summary>El plazo para dar un cobro por perdido debe ser mayor que cero.</summary>
    public static readonly Error StalePaymentAgeMustBePositive = new("293");
}
