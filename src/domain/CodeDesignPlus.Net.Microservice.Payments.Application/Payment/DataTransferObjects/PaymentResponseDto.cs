using System;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

public class PaymentResponseDto
{
    public bool Succeeded { get; set; }
    public PaymentStatus Status { get; set; }
    public string TransactionId { get; set; } = null!;
    public string Message { get; set; }= null!;
    public string RawResponse { get; set; }= null!;
    public string? RedirectionUrl { get; set; }
    public FinancialNetwork FinancialNetwork { get; set; } = null!;
}
