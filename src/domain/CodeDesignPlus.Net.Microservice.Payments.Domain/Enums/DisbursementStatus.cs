namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Represents the status of a disbursement.
/// </summary>
public enum DisbursementStatus
{
    /// <summary>
    /// The disbursement has been created and is awaiting processing.
    /// </summary>
    Pending,
    /// <summary>
    /// The disbursement is currently being processed by the payment provider.
    /// </summary>
    Processing,
    /// <summary>
    /// The disbursement has been completed successfully.
    /// </summary>
    Completed,
    /// <summary>
    /// The disbursement has failed.
    /// </summary>
    Failed
}
