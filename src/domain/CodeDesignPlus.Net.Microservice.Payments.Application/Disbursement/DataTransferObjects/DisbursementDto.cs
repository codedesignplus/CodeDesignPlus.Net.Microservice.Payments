using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.DataTransferObjects;

public class DisbursementDto : IDtoBase
{
    public required Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid BeneficiaryId { get; set; }
    public Guid BeneficiaryUserId { get; set; }
    public long TotalAmount { get; set; }
    public long CommissionAmount { get; set; }
    public long DisbursedAmount { get; set; }
    public string Currency { get; set; } = null!;
    public DisbursementStatus Status { get; set; }
    public Instant? ProcessedAt { get; set; }
    public string? ProviderReference { get; set; }
    public string? FailureReason { get; set; }
    public Guid Tenant { get; set; }
}
