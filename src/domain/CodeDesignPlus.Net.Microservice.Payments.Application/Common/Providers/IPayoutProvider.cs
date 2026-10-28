using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;

/// <summary>
/// Pays third parties out of the merchant account.
/// </summary>
/// <remarks>
/// Each payout goes alone in its own batch. Requesting it is not paying it: the provider confirms it later by event,
/// and it may wait for an approver of the tenant on the provider's panel.
/// </remarks>
public interface IPayoutProvider : IPaymentProvider
{
    /// <summary>
    /// Sends a payout to the provider.
    /// </summary>
    /// <param name="order">What to pay and to whom.</param>
    /// <param name="credentials">The merchant account that pays; it must have payouts connected.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ids the provider gave it and its status.</returns>
    Task<PayoutSubmission> SubmitAsync(PayoutOrder order, MerchantCredentials credentials, CancellationToken cancellationToken);

    /// <summary>
    /// Banks the provider can pay to.
    /// </summary>
    /// <param name="credentials">The merchant account that pays; it must have payouts connected.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The banks, with the id the provider expects in a payout.</returns>
    Task<IReadOnlyList<PayoutBank>> GetBanksAsync(MerchantCredentials credentials, CancellationToken cancellationToken);
}

/// <summary>
/// A payout as the provider needs it.
/// </summary>
/// <param name="PayoutId">Id of the payout; it is the idempotency key and the reference of the batch.</param>
/// <param name="Amount">Amount to pay.</param>
/// <param name="Beneficiary">Bank account that receives it. Lives only in memory while the payout is sent.</param>
public sealed record PayoutOrder(Guid PayoutId, Money Amount, PayoutBeneficiaryAccount Beneficiary);

/// <summary>
/// Bank account of a beneficiary, as the provider needs it.
/// </summary>
/// <param name="LegalIdType">Type of the holder's document, in the provider's code.</param>
/// <param name="LegalId">Number of the holder's document.</param>
/// <param name="BankId">Bank, in the provider's code.</param>
/// <param name="AccountType">Type of account, in the provider's code.</param>
/// <param name="AccountNumber">Account number.</param>
/// <param name="Name">Holder of the account.</param>
/// <param name="Email">Where the provider notifies the beneficiary.</param>
public sealed record PayoutBeneficiaryAccount(string LegalIdType, string LegalId, string BankId, string AccountType, string AccountNumber, string Name, string Email)
{
    /// <summary>
    /// Never prints the account number.
    /// </summary>
    /// <returns>A redacted description.</returns>
    public override string ToString() => $"{nameof(PayoutBeneficiaryAccount)} {{ BankId = {BankId}, AccountType = {AccountType} }}";
}

/// <summary>
/// What the provider answered when it received a payout.
/// </summary>
/// <param name="BatchId">Id of the provider's batch.</param>
/// <param name="TransactionId">Id of the transaction inside the batch, when the provider returns it.</param>
/// <param name="Status">Normalized status.</param>
public sealed record PayoutSubmission(string BatchId, string? TransactionId, PayoutStatus Status);

/// <summary>
/// A bank the provider can pay to.
/// </summary>
/// <param name="Id">Id the provider expects in a payout.</param>
/// <param name="Name">Name of the bank.</param>
public sealed record PayoutBank(string Id, string Name);
