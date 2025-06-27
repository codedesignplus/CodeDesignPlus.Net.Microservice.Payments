using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

public class PaymentDto : IDtoBase
{
    public required Guid Id { get; set; }
    public string Module { get; private set; } = null!;
    public Guid? Tenant { get; private set; }
    public PaymentStatus Status { get; private set; }
    public Amount SubTotal { get; private set; } = null!;
    public Amount Tax { get; private set; } = null!;
    public Amount Total { get; private set; } = null!;
    public Payer Payer { get; private set; } = null!;
    public Domain.ValueObjects.PaymentMethod PaymentMethod { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    public PaymentProvider PaymentProvider { get; private set; } = PaymentProvider.None;
    public string? ProviderTransactionId { get; private set; }
    public string? ProviderResponseMessage { get; private set; }
    public string? RawProviderResponseData { get; private set; }
    public FinancialNetwork FinancialNetwork { get; private set; } = null!;
}