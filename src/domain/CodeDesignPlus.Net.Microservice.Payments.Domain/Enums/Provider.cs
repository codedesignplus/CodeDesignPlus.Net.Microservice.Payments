namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Represents the payment providers available in the system.
/// </summary>
public enum Provider
{
    /// <summary>
    /// No payment provider selected.
    /// </summary>
    None,
    /// <summary>
    /// Represents the PayU payment provider.
    /// </summary>
    Payu,
    /// <summary>
    /// Represents the MercadoPago payment provider.
    /// </summary>
    MercadoPago
}
