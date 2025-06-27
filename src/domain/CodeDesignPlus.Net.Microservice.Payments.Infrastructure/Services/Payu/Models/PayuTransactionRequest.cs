namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class PayuTransactionRequest
{
    public bool Test { get; set; }
    public string Language { get; set; } = null!;
    public string Command { get; set; } = null!;
    public Merchant Merchant { get; set; } = null!;
    public TransactionDetails Details { get; set; } = null!;
}

public class TransactionDetails
{
    public string TransactionId { get; set; } = null!;
}