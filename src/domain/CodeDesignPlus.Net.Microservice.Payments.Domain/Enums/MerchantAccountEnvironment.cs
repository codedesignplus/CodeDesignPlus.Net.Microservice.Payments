namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Provider environment a set of merchant keys belongs to.
/// </summary>
public enum MerchantAccountEnvironment
{
    /// <summary>
    /// Test keys: payments are simulated and no money moves.
    /// </summary>
    Sandbox = 1,
    /// <summary>
    /// Live keys: payments move real money.
    /// </summary>
    Production = 2
}
