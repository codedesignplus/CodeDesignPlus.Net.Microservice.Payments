namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Why a payment did not succeed, normalized from whatever the provider reports and free of any provider's names.
/// </summary>
/// <remarks>
/// Each provider adapter translates its own statuses to these values, and the screen picks the text it shows by value
/// (pendings/622): a declined card is not a session that lapsed.
/// </remarks>
public enum PaymentFailureReason
{
    /// <summary>
    /// The payment did not fail, or it closed before the reason was recorded.
    /// </summary>
    None = 0,
    /// <summary>
    /// The bank, the card network or the provider's fraud rules rejected the transaction.
    /// </summary>
    Declined = 1,
    /// <summary>
    /// The transaction was voided before it settled.
    /// </summary>
    Voided = 2,
    /// <summary>
    /// The provider could not process the transaction because of an error on its side or on the processor's.
    /// </summary>
    ProviderError = 3,
    /// <summary>
    /// The payment session lapsed before the payer completed it.
    /// </summary>
    Expired = 4
}
