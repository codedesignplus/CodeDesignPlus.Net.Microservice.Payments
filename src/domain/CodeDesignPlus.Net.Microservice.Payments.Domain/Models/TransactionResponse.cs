using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Models;

public class TransactionResponse
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = null!;
    public object Request { get; set; } = null!;
    public object Response { get; set; } = null!;
}
