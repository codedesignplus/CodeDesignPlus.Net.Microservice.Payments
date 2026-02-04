namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class Result<T>
{
    public T Payload { get; set; } = default!;
}
