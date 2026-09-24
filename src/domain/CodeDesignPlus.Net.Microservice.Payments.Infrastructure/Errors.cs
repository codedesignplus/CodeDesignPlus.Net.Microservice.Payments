using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure;

public class Errors : IErrorCodes
{
    public static readonly Error UnknownError = new("300");
    public static readonly Error CreditCardCannotBeNull = new("301");
    public static readonly Error PseCannotBeNull = new("302");

    public static readonly Error InvalidId = new("303");

    public static readonly Error ReferenceCodeIsInvalid = new("304");

    public static readonly Error MerchantIdIsRequired = new("305");
    public static readonly Error CurrencyIsRequired = new("306");
    public static readonly Error StateIsRequired = new("307");
    public static readonly Error ValueIsRequired = new("308");
    public static readonly Error SignatureIsRequired = new("309");
    public static readonly Error ReferenceCodeIsRequired = new("310");

    public static readonly Error MerchantIdMissing = new("311");
    public static readonly Error CurrencyMissing = new("312");
    public static readonly Error StateMissing = new("313");
    public static readonly Error ValueMissing = new("314");
    public static readonly Error SignatureMissing = new("315");
    public static readonly Error ReferenceSaleMissing = new("316");

    public static readonly Error InvalidReferenceSale = new("317");

    public static readonly Error CreditCardTokenizationFailed = new("318");

    public static readonly Error PayerEmailAddressCannotBeNullOrEmpty = new("319");
    public static readonly Error PayerContactPhoneCannotBeNullOrEmpty = new("320");

    /// <summary>La pasarela rechazo la peticion y dijo por que.</summary>
    /// <remarks>
    /// Quien la lanza le concatena el motivo que reporto PayU, que es dinamico. Asi el codigo se mantiene
    /// estable para el cliente y el detalle real llega al ProblemDetails, en vez de quedarse en un log.
    /// </remarks>
    public static readonly Error PaymentGatewayRejectedTheRequest = new("321");

    public static readonly Error PaymentMethodCodeIsNotValid = new("322");
}
