namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Lifecycle of a payment to a third party.
/// </summary>
public enum PayoutStatus
{
    /// <summary>
    /// A module asked for the payout; it has not been sent to the provider yet.
    /// </summary>
    Requested = 1,
    /// <summary>
    /// The provider received it and waits for an approver of the tenant to approve it on its panel.
    /// </summary>
    AwaitingApproval = 2,
    /// <summary>
    /// Approved and being processed by the provider.
    /// </summary>
    Processing = 3,
    /// <summary>
    /// The provider confirmed the money reached the beneficiary.
    /// </summary>
    Completed = 4,
    /// <summary>
    /// The provider rejected or could not complete the payout.
    /// </summary>
    Failed = 5
}
