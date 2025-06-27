using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class PayuTransactionResponse
{
    public string Code { get; set; } = null!;
    public string? Error { get; set; }
    public Result<TransactionResponse> Result { get; set; } = null!;
}
