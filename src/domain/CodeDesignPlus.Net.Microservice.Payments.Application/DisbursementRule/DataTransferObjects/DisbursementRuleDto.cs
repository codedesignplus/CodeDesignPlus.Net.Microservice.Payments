using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.DataTransferObjects;

public class DisbursementRuleDto : IDtoBase
{
    public required Guid Id { get; set; }
    public CommissionType CommissionType { get; set; }
    public long CommissionAmount { get; set; }
    public string Currency { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public Guid Tenant { get; set; }
}
