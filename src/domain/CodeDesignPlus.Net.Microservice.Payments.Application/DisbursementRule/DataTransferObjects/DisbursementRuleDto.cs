using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.DataTransferObjects;

public class DisbursementRuleDto : IDtoBase
{
    public required Guid Id { get; set; }
    public CommissionType CommissionType { get; set; }
    /// <summary>Comisión fija en unidades menores. Null cuando la comisión es porcentual.</summary>
    public long? FixedCommission { get; set; }
    /// <summary>Moneda ISO 4217 de <see cref="FixedCommission"/>. Null cuando la comisión es porcentual.</summary>
    public string? Currency { get; set; }
    /// <summary>Comisión en puntos base (200 = 2%). Null cuando la comisión es fija.</summary>
    public int? CommissionBasisPoints { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public Guid Tenant { get; set; }
}
