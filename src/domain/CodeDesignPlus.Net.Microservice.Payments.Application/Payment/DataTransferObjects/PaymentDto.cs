namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

public class PaymentDto: IDtoBase
{
    public required Guid Id { get; set; }
}