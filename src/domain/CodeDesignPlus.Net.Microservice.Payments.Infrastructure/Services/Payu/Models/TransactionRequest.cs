using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

/// <summary>
/// Represents a request to submit a transaction to the PayU payment gateway.
/// </summary>
public class TransactionRequest
{
    /// <summary>
    /// Language used in the request, this language is used to display error messages generated.
    /// </summary>
    public string Language { get; set; } = null!;
    /// <summary>
    /// Assigns TRANSACTION_RESPONSE_DETAIL.
    /// </summary>
    public string Command { get; } = "TRANSACTION_RESPONSE_DETAIL";
    /// <summary>
    /// Assigns true if the request is in test mode. If not, assign false.
    /// </summary>
    public bool Test { get; set; } = false;
    /// <summary>
    /// This object contains the authentication data.
    /// </summary>
    public PayuMerchant Merchant { get; set; } = null!;
    /// <summary>
    /// This object contains the transaction details.
    /// </summary>
    public TransactionDetail Details { get; set; } = null!;
}

/// <summary>
/// Represents the merchant's authentication data for the PayU payment gateway.
/// </summary>
public class TransactionDetail
{
    /// <summary>
    /// The unique identifier of the transaction.
    /// </summary>
    public string TransactionId { get; set; } = null!;
}
