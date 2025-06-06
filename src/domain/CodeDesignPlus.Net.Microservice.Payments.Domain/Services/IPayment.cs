using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Models;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Services;

/// <summary>
/// Defines the contract for payment processing services.
/// </summary>
public interface IPayment
{
    /// <summary>
    /// Processes a payment.
    /// </summary>
    /// <param name="id">The unique identifier for the payment transaction.</param>
    /// <param name="transaction">The transaction details including order, payer, payment method, and credit card information.</param>
    /// <param name="provider">The payment provider to use for processing the transaction.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the transaction response.</returns>
    Task<TransactionResponse> ProcessPayment(Guid id, Transaction transaction, Provider provider, CancellationToken cancellationToken);
    /// <summary>
    /// Processes a payment.
    /// </summary>
    /// <param name="id">The unique identifier for the payment transaction.</param>
    /// <param name="transaction">The transaction details including order, payer, payment method, and credit card information.</param>
    /// <param name="provider">The payment provider to use for processing the transaction.</param>
    /// <param name="extraParametrs">Additional parameters that may be required for processing the payment.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the transaction response.</returns>
    Task<TransactionResponse> ProcessPayment(Guid id, Transaction transaction, Provider provider, Dictionary<string, string> extraParametrs, CancellationToken cancellationToken);
}
