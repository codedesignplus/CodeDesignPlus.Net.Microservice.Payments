using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Models;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

public class PaymentDto : IDtoBase
{
    public required Guid Id { get; set; }
    public Provider Provider { get; set; } = Provider.None;
    public Transaction Transaction { get; set; } = null!;
    public string Request { get; set; } = null!;
    public TransactionResponseData Response { get; set; } = null!;
}