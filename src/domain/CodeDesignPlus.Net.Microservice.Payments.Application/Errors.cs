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

    // 219 y 220 eran de la configuración de pasarela por copropiedad, retirada en el plan 186. No se reutilizan.

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

    /// <summary>Para listar los cobros hace falta la copropiedad en sesión.</summary>
    public static readonly Error TenantIsRequiredToListPayments = new("294");

    // Collection accounts, providers and payouts (pendings/660)
    public static readonly Error MerchantAccountIsNotActive = new("340");
    public static readonly Error PlatformAccountIsNotConfigured = new("341");
    public static readonly Error TenantIsRequiredToCharge = new("342");
    public static readonly Error NoPaymentProviderForCountry = new("343");
    public static readonly Error MerchantKeysWereRejected = new("344");
    public static readonly Error MerchantKeysEnvironmentMismatch = new("345");
    public static readonly Error PaymentProviderDoesNotMatchAccount = new("346");
    public static readonly Error PayoutNotFound = new("347");
    public static readonly Error PayoutsAreNotEnabled = new("348");
    public static readonly Error PayoutsCredentialsAreIncomplete = new("349");
    public static readonly Error TenantIsRequired = new("350");
    public static readonly Error PaymentProviderCannotPayThirdParties = new("351");

    // Keys rejected by the provider (pendings/660)
    public static readonly Error MerchantKeysRejectedByProvider = new("352");

    // Why a collection account was invalidated (pendings/660)
    public static readonly Error MerchantPublicKeyIsNotFromProvider = new("353");
    public static readonly Error MerchantPrivateKeyDoesNotMatchPublicKey = new("354");
    public static readonly Error MerchantPublicKeyIsUnknown = new("355");
    public static readonly Error MerchantIsInactive = new("356");
    public static readonly Error MerchantPrivateKeyWasRejected = new("357");
    public static readonly Error MerchantEventsSecretDoesNotVerify = new("358");

    // Provider fee (pendings/660, stage 6)
    public static readonly Error MerchantAccountNotFound = new("371");
}
