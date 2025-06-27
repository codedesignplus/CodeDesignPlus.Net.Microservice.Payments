using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

public class PaymentDto : IDtoBase
{
    public required Guid Id { get; set; }
    public string Module { get; set; } = null!;
    public Guid? Tenant { get; set; }
    public PaymentStatus Status { get; set; }
    public Amount SubTotal { get; set; } = null!;
    public Amount Tax { get; set; } = null!;
    public Amount Total { get; set; } = null!;
    public Payer Payer { get; set; } = null!;
    public Domain.ValueObjects.PaymentMethod PaymentMethod { get; set; } = null!;
    public string Description { get; set; } = null!;

    public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.None;
    public string? ProviderTransactionId { get; set; }
    public string? ProviderResponseMessage { get; set; }
    public string? RawProviderResponseData { get; set; }
    public FinancialNetwork FinancialNetwork { get; set; } = null!;
}