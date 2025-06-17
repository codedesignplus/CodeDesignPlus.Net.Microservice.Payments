using System;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Models;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class TransactionResponse
{
    public string Code { get; set; } = null!;
    public string Error { get; set; } = null!;
    public TransactionResult Result { get; set; } = null!;
}

public class TransactionResult
{
    public TransactionResponseDetails Payload { get; set; } = null!;
}
