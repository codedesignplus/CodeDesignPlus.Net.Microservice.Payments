namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Represents the type of commission applied in a disbursement rule.
/// </summary>
public enum CommissionType
{
    /// <summary>
    /// A fixed amount deducted as commission.
    /// </summary>
    Fixed,
    /// <summary>
    /// A percentage-based commission (in basis points, e.g., 200 = 2%).
    /// </summary>
    Percentage
}
