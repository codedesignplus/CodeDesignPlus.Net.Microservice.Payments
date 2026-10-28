namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Lifecycle of the collection account a tenant connects with its payment provider.
/// </summary>
public enum MerchantAccountStatus
{
    /// <summary>
    /// The account exists but the tenant has not connected its provider keys yet.
    /// </summary>
    PendingConnection = 1,
    /// <summary>
    /// The keys were validated against the provider: the tenant can charge online.
    /// </summary>
    Active = 2,
    /// <summary>
    /// The provider stopped accepting the stored keys (rotated or revoked on its panel).
    /// </summary>
    Invalid = 3,
    /// <summary>
    /// The tenant disconnected the account and its keys were deleted.
    /// </summary>
    Disabled = 4
}
