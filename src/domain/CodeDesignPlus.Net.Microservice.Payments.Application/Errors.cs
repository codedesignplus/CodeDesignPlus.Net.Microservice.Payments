namespace CodeDesignPlus.Net.Microservice.Payments.Application;

public class Errors: IErrorCodes
{    
    public const string UnknownError = "200 : UnknownError";
    public const string InvalidRequest = "201 : Invalid Request";
    public const string PaymentAlredyExists = "202 : Payment already exists";
    public const string PaymentNotFound = "203 : Payment not found";

    public const string PaymentMethodAlreadyExists = "204 : Payment method already exists";

    public const string PaymentMethodNotFound = "205 : Payment method not found";
}
