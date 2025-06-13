using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Models;

/// <summary>
/// Represents a transaction response in the payment system.
/// Contains details about the transaction, including the provider, request, and response data.
/// </summary>
public class TransactionResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for the transaction response.
    /// This identifier is used to uniquely identify the transaction response in the system.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the provider associated with the transaction response.
    /// This field indicates which payment provider processed the transaction, such as PayU, Stripe, etc.
    /// </summary>
    public string Provider { get; set; } = null!;
    /// <summary>
    /// Gets or sets the module associated with the transaction response.
    /// This field indicates the module or component of the system that initiated the transaction.
    /// </summary>
    public object Request { get; set; } = null!;
    /// <summary>
    /// Gets or sets the response data for the transaction.
    /// This field contains the details of the transaction response, including the transaction state, response codes, and additional information.
    /// </summary>
    public TransactionResponseData Response { get; set; } = null!;
}

/// <summary>
/// Represents the response data for a transaction in the payment system.
/// Contains details about the transaction, including its state, response codes, and additional information.
/// </summary>
public class TransactionResponseData
{
    /// <summary>
    /// Gets or sets the code indicating the response status of the transaction.
    /// Possible values are "ERROR" and "SUCCESS".
    /// </summary>
    public string Code { get; set; } = null!;
    /// <summary>
    /// Gets or sets the error message associated with the transaction response.
    /// This field is populated when the response code is "ERROR".
    /// </summary>
    public string? Error { get; set; }
    /// <summary>
    /// Gets or sets the details of the transaction response.
    /// </summary>
    public TransactionResponseDetails TransactionResponse { get; set; } = null!;
}

/// <summary>
/// Represents the details of a transaction response in the payment system.
/// Contains information such as order ID, transaction ID, state, response codes, and additional parameters.
/// </summary>
public class TransactionResponseDetails
{
    /// <summary>
    /// Gets or sets the unique identifier for the order in the payment gateway.
    /// </summary>
    public long OrderId { get; set; }
    /// <summary>
    /// Gets or sets the unique identifier for the transaction in the payment gateway.
    /// </summary>
    public string TransactionId { get; set; } = null!;
    /// <summary>
    /// Gets or sets the state of the transaction.
    /// Possible values include "PENDING", "APPROVED", "DECLINED", etc. 
    /// </summary>
    public string State { get; set; } = null!;
    /// <summary>
    /// Gets or sets the response code associated with the transaction state.
    /// </summary>
    public string ResponseCode { get; set; } = null!;
    /// <summary>
    /// Gets or sets the response code returned by the payment network.
    /// </summary>
    public string PaymentNetworkResponseCode { get; set; } = null!;
    /// <summary>
    /// Gets or sets the error message returned by the payment network, if any.
    /// This field is populated when there is an error in the payment network response.
    /// </summary>
    public string PaymentNetworkResponseErrorMessage { get; set; } = null!;
    /// <summary>
    /// Gets or sets the traceability code returned by the payment network.
    /// </summary>
    public string TrazabilityCode { get; set; } = null!;
    /// <summary>
    /// Gets or sets the authorization code returned by the payment network.
    /// This code is typically used to authorize the transaction with the bank or payment provider.
    /// </summary>
    public string AuthorizationCode { get; set; } = null!;
    /// <summary>
    /// Gets or sets the message associated with the response code of the transaction.
    /// This message provides additional context or information about the transaction status.
    /// </summary>
    public string ResponseMessage { get; set; } = null!;
    /// <summary>
    /// Gets or sets the date and time when the transaction response was created in the payment system.
    /// This field is typically used for logging and auditing purposes.
    /// </summary>
    public DateTime OperationDate { get; set; }
    /// <summary>
    /// Gets or sets additional parameters or data associated with the transaction response.
    /// This field can contain various key-value pairs that provide extra context or information about the transaction.
    /// </summary>
    public Dictionary<string, string> ExtraParameters { get; set; } = null!;
    /// <summary>
    /// Gets or sets additional information related to the transaction response.
    /// This object has the same structure as ExtraParameters and can contain various key-value pairs
    /// </summary>
    public Dictionary<string, string> AdditionalInfo { get; set; } = null!;
}
