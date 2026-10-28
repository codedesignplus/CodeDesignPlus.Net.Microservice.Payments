namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

/// <summary>
/// Why money leaves the tenant's collection account.
/// </summary>
public enum PayoutPurpose
{
    /// <summary>
    /// Pays a supplier's expense.
    /// </summary>
    Expense = 1,
    /// <summary>
    /// Hands collected money over to its owner.
    /// </summary>
    OwnerDisbursement = 2,
    /// <summary>
    /// Moves the balance to the tenant's own bank account.
    /// </summary>
    Withdrawal = 3
}
