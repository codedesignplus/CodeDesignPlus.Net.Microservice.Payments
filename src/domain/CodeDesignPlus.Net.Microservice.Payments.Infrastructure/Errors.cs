namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure;

public class Errors: IErrorCodes
{    
    public const string UnknownError = "300 : UnknownError";
    public const string CreditCardCannotBeNull = "301 : Credit Card Cannot Be Null";
    public const string PseCannotBeNull = "302 : Pse Cannot Be Null";

    public const string InvalidId = "303 : Invalid Id";
}
