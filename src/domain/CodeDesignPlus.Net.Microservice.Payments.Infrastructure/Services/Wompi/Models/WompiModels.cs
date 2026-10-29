using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Logging;
using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi.Models;

/// <summary>
/// The envelope of every Wompi response.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
public sealed class WompiResponse<T>
{
    /// <summary>
    /// The data of the response.
    /// </summary>
    [JsonProperty("data")]
    public T? Data { get; set; }
}

/// <summary>
/// A merchant, as <c>GET /merchants/{public key}</c> returns it.
/// </summary>
public sealed class WompiMerchant
{
    /// <summary>
    /// Id of the merchant at Wompi.
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Trade name of the merchant.
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Legal name of the merchant.
    /// </summary>
    [JsonProperty("legal_name")]
    public string? LegalName { get; set; }

    /// <summary>
    /// Whether the merchant is active.
    /// </summary>
    [JsonProperty("active")]
    public bool Active { get; set; }
}

/// <summary>
/// A refund, as Wompi reports it in its <c>refund.updated</c> event (docs/wompi/reembolsos.md).
/// </summary>
public sealed class WompiRefund
{
    /// <summary>
    /// Id of the refund at Wompi.
    /// </summary>
    [JsonProperty("refund_id")]
    public string? RefundId { get; set; }

    /// <summary>
    /// Id of the transaction refunded.
    /// </summary>
    [JsonProperty("transaction_id")]
    public string? TransactionId { get; set; }

    /// <summary>
    /// PENDING, APPROVED, DECLINED, CANCELLED or ERROR.
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Amount refunded, in cents.
    /// </summary>
    [JsonProperty("amount_in_cents")]
    public long? AmountInCents { get; set; }

    /// <summary>
    /// ISO 4217 currency.
    /// </summary>
    [JsonProperty("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// A reference of the refund, which is not necessarily the one of the transaction.
    /// </summary>
    [JsonProperty("reference")]
    public string? Reference { get; set; }

    /// <summary>
    /// Code of the reason, when the refund did not go through.
    /// </summary>
    [JsonProperty("code")]
    public string? Code { get; set; }

    /// <summary>
    /// The reason in Wompi's words, when the refund did not go through.
    /// </summary>
    [JsonProperty("message")]
    public string? Message { get; set; }
}

/// <summary>
/// A collection transaction, as Wompi reports it in its API and in its events.
/// </summary>
public sealed class WompiTransaction
{
    /// <summary>
    /// Id of the transaction at Wompi.
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The reference: the id of the payment in this service.
    /// </summary>
    [JsonProperty("reference")]
    public string? Reference { get; set; }

    /// <summary>
    /// PENDING, APPROVED, DECLINED, VOIDED or ERROR.
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Why the transaction ended as it did, in Wompi's words.
    /// </summary>
    [JsonProperty("status_message")]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Amount charged, in cents.
    /// </summary>
    [JsonProperty("amount_in_cents")]
    public long? AmountInCents { get; set; }

    /// <summary>
    /// ISO 4217 currency.
    /// </summary>
    [JsonProperty("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// Payment method the payer chose (CARD, PSE, NEQUI…).
    /// </summary>
    [JsonProperty("payment_method_type")]
    public string? PaymentMethodType { get; set; }

    /// <summary>
    /// When the transaction was created, as Wompi wrote it.
    /// </summary>
    [JsonProperty("created_at")]
    public string? CreatedAt { get; set; }

    /// <summary>
    /// When the transaction reached its final status, as Wompi wrote it.
    /// </summary>
    [JsonProperty("finalized_at")]
    public string? FinalizedAt { get; set; }
}

/// <summary>
/// A batch of payouts, as <c>POST /payouts</c> receives it. Wompi always takes a batch; this service sends one
/// transaction per batch.
/// </summary>
public sealed class WompiPayoutRequest
{
    /// <summary>
    /// Reference of the batch.
    /// </summary>
    [JsonProperty("reference")]
    public string Reference { get; set; } = null!;

    /// <summary>
    /// Wompi account the money leaves from.
    /// </summary>
    [JsonProperty("accountId")]
    public string AccountId { get; set; } = null!;

    /// <summary>
    /// Kind of payment; payments to suppliers are <c>PROVIDERS</c>.
    /// </summary>
    [JsonProperty("paymentType")]
    public string PaymentType { get; set; } = null!;

    /// <summary>
    /// The transactions of the batch.
    /// </summary>
    [JsonProperty("transactions")]
    public List<WompiPayoutTransaction> Transactions { get; set; } = [];
}

/// <summary>
/// One payment inside a payouts batch.
/// </summary>
public sealed class WompiPayoutTransaction
{
    /// <summary>
    /// Type of the holder's document (CC, NIT, CE…).
    /// </summary>
    [JsonProperty("legalIdType")]
    public string LegalIdType { get; set; } = null!;

    /// <summary>
    /// Number of the holder's document.
    /// </summary>
    [NotLogged]
    [JsonProperty("legalId")]
    public string LegalId { get; set; } = null!;

    /// <summary>
    /// Bank, as <c>GET /banks</c> names it.
    /// </summary>
    [JsonProperty("bankId")]
    public string BankId { get; set; } = null!;

    /// <summary>
    /// AHORROS or CORRIENTE.
    /// </summary>
    [JsonProperty("accountType")]
    public string AccountType { get; set; } = null!;

    /// <summary>
    /// Account number.
    /// </summary>
    [NotLogged]
    [JsonProperty("accountNumber")]
    public string AccountNumber { get; set; } = null!;

    /// <summary>
    /// Holder of the account.
    /// </summary>
    [NotLogged]
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Where Wompi notifies the beneficiary.
    /// </summary>
    [NotLogged]
    [JsonProperty("email")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Amount in cents.
    /// </summary>
    [JsonProperty("amount")]
    public long Amount { get; set; }

    /// <summary>
    /// Reference of the transaction.
    /// </summary>
    [JsonProperty("reference")]
    public string Reference { get; set; } = null!;
}

/// <summary>
/// A payouts batch, as Wompi answers it.
/// </summary>
public sealed class WompiPayoutBatch
{
    /// <summary>
    /// Id of the batch.
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Status of the batch; a new one is PENDING_APPROVAL.
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; set; }
}

/// <summary>
/// A bank Wompi can pay to.
/// </summary>
public sealed class WompiBank
{
    /// <summary>
    /// Id Wompi expects in a payout.
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Name of the bank.
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }
}
