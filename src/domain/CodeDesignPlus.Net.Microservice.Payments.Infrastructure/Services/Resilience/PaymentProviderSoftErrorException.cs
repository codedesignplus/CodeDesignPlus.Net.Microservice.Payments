namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Resilience;

public class PaymentProviderSoftErrorException(string message, string responseBody) : Exception(message)
{
    public string ResponseBody { get; } = responseBody;
}
