using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Models;

public class TransactionResponse
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = null!;
    public string Request { get; set; } = null!;
    public string Response { get; set; } = null!;
}
