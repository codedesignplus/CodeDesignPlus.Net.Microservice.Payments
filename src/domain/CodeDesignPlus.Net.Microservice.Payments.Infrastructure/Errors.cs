using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure;

public class Errors : IErrorCodes
{
    public static readonly Error UnknownError = new("300", "UnknownError");
    public static readonly Error CreditCardCannotBeNull = new("301", "Credit Card Cannot Be Null");
    public static readonly Error PseCannotBeNull = new("302", "Pse Cannot Be Null");

    public static readonly Error InvalidId = new("303", "Invalid Id");

    public static readonly Error ReferenceCodeIsInvalid = new("304", "Reference Code Is Invalid");

    public static readonly Error MerchantIdIsRequired = new("305", "Merchant Id Is Required");
    public static readonly Error CurrencyIsRequired = new("306", "Currency Is Required");
    public static readonly Error StateIsRequired = new("307", "State Is Required");
    public static readonly Error ValueIsRequired = new("308", "Value Is Required");
    public static readonly Error SignatureIsRequired = new("309", "Signature Is Required");
    public static readonly Error ReferenceCodeIsRequired = new("310", "Reference Code Is Required");

    public static readonly Error MerchantIdMissing = new("311", "Merchant Id Missing");
    public static readonly Error CurrencyMissing = new("312", "Currency Missing");
    public static readonly Error StateMissing = new("313", "State Missing");
    public static readonly Error ValueMissing = new("314", "Value Missing");
    public static readonly Error SignatureMissing = new("315", "Signature Missing");
    public static readonly Error ReferenceSaleMissing = new("316", "Reference Sale Missing");

    public static readonly Error InvalidReferenceSale = new("317", "Invalid Reference Sale");

    public static readonly Error CreditCardTokenizationFailed = new("318", "Credit Card Tokenization Failed");

    public static readonly Error PayerEmailAddressCannotBeNullOrEmpty = new("319", "Payer Email Address Cannot Be Null Or Empty");
    public static readonly Error PayerContactPhoneCannotBeNullOrEmpty = new("320", "Payer Contact Phone Cannot Be Null Or Empty");

    /// <summary>La pasarela rechazo la peticion y dijo por que.</summary>
    /// <remarks>
    /// Quien la lanza le concatena el motivo que reporto PayU, que es dinamico. Asi el codigo se mantiene
    /// estable para el cliente y el detalle real llega al ProblemDetails, en vez de quedarse en un log.
    /// </remarks>
    public static readonly Error PaymentGatewayRejectedTheRequest = new("321", "The payment gateway rejected the request.");

    public static readonly Error PaymentMethodCodeIsNotValid = new("301", "The payment method code '{0}' is not valid.");
}
