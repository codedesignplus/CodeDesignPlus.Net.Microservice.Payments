using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

public class PaymentDto : IDtoBase
{
    public required Guid Id { get; set; }
    public string Module { get; set; } = null!;
    public Guid? Tenant { get; set; }
    public PaymentStatus Status { get; set; }
    public Net.ValueObjects.Financial.Money SubTotal { get; set; } = null!;
    public Net.ValueObjects.Financial.Money Tax { get; set; } = null!;
    public Net.ValueObjects.Financial.Money Total { get; set; } = null!;
    public Payer Payer { get; set; } = null!;
    public Domain.ValueObjects.PaymentMethod PaymentMethod { get; set; } = null!;
    public string Description { get; set; } = null!;

    public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.None;
    public Dictionary<string, string?> InitiateResponse { get; set; } = [];
    public Dictionary<string, string?> FinalResponse { get; set; } = [];
}