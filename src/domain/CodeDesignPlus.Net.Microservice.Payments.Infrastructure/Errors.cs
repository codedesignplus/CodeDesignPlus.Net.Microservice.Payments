namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure;

public class Errors : IErrorCodes
{
    public const string UnknownError = "300 : UnknownError";
    public const string CreditCardCannotBeNull = "301 : Credit Card Cannot Be Null";
    public const string PseCannotBeNull = "302 : Pse Cannot Be Null";

    public const string InvalidId = "303 : Invalid Id";

    public const string ReferenceCodeIsInvalid = "304 : Reference Code Is Invalid";

    public const string MerchantIdIsRequired = "305 : Merchant Id Is Required";
    public const string CurrencyIsRequired = "306 : Currency Is Required";
    public const string StateIsRequired = "307 : State Is Required";
    public const string ValueIsRequired = "308 : Value Is Required";
    public const string SignatureIsRequired = "309 : Signature Is Required";
    public const string ReferenceCodeIsRequired = "310 : Reference Code Is Required";

    public const string MerchantIdMissing = "311 : Merchant Id Missing";
    public const string CurrencyMissing = "312 : Currency Missing";
    public const string StateMissing = "313 : State Missing";
    public const string ValueMissing = "314 : Value Missing";
    public const string SignatureMissing = "315 : Signature Missing";
    public const string ReferenceSaleMissing = "316 : Reference Sale Missing";

    public const string InvalidReferenceSale = "317 : Invalid Reference Sale";
}
