namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Represents the type of payment method.
/// This enum is used to categorize different payment methods available in the system.
/// </summary>
public enum TypePaymentMethod
{
    /// <summary>
    /// Represents no specific payment method.
    /// </summary>
    None,
    /// <summary>
    /// Represents a payment method using a credit card.
    /// </summary>
    CreditCard,
    /// <summary>
    /// Represents a payment method using a debit card.
    /// </summary>
    DebitCard,
    /// <summary>
    /// Represents a payment method using a bank reference.
    /// </summary>
    BankReference,
    /// <summary>
    /// Represents a payment method using a bank transfer.
    /// </summary>
    BankTransfer,
    /// <summary>
    /// Represents a payment method using a digital wallet or e-wallet.
    /// </summary>
    MobilePaymentService,
    /// <summary>
    /// Represents a payment method using cash.
    /// </summary>
    Cash
}
