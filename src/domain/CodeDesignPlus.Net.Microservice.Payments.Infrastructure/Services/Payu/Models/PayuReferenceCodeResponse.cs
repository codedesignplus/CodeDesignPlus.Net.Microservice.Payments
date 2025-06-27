using System;
using System.Collections.Generic;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class PayuReferenceCodeResponse
{
    public string Code { get; set; } = null!;
    public string? Error { get; set; }
    public Result<Payload> Result { get; set; } = null!;
}

public class Payload
{
    public long Id { get; set; }
    public int AccountId { get; set; }
    public string Status { get; set; } = null!;
    public string ReferenceCode { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string AirlineCode { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string NotifyUrl { get; set; } = null!;
    public string ShippingAddress { get; set; } = null!;
    public Buyer Buyer { get; set; } = null!;
    public string AntifraudMerchantId { get; set; } = null!;
    public bool IsTest { get; set; }
    public List<Transaction> Transactions { get; set; } = null!;
    public Dictionary<string, AdditionalValueDetail> AdditionalValues { get; set; } = null!;
    public long CreationDate { get; set; }
    public bool IsCreatedUsingStandardIntegrationParams { get; set; }
    public int MerchantId { get; set; }
    public string ProcessedTransactionId { get; set; } = null!;
    public string OrderSignature { get; set; } = null!;
}

public class Buyer
{
    public string MerchantBuyerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string ContactPhone { get; set; } = null!;
    public string BuyerAddress { get; set; } = null!;
    public string DniNumber { get; set; } = null!;
    public string Cnpj { get; set; } = null!;
}

public class Transaction
{
    public string Id { get; set; } = null!;
    public string Order { get; set; } = null!;
    public CreditCard CreditCard { get; set; }  = null!;
    public string BankAccount { get; set; }  = null!;
    public string Type { get; set; }  = null!;
    public string ParentTransactionId { get; set; }  = null!;
    public string PaymentMethod { get; set; }  = null!;
    public string Source { get; set; }  = null!;
    public string PaymentCountry { get; set; }  = null!;
    public TransactionResponse TransactionResponse { get; set; }  = null!;
    public string DeviceSessionId { get; set; }  = null!;
    public string IpAddress { get; set; }  = null!;
    public string Cookie { get; set; }  = null!;
    public string UserAgent { get; set; }  = null!;
    public string ExpirationDate { get; set; }  = null!;
    public Payer Payer { get; set; }  = null!;
    public int TermsAndConditionId { get; set; }
    public Dictionary<string, AdditionalValueDetail> AdditionalValues { get; set; } = null!;
    public Dictionary<string, string> ExtraParameters { get; set; } = null!;
}

public class CreditCard
{
    public string MaskedNumber { get; set; } = null!;
    public string IssuerBank { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string CardType { get; set; } = null!;
}

public class TransactionResponse
{
    public string State { get; set; } = null!;
    public string PaymentNetworkResponseCode { get; set; } = null!;
    public string PaymentNetworkResponseErrorMessage { get; set; } = null!;
    public string TrazabilityCode { get; set; } = null!;
    public string AuthorizationCode { get; set; } = null!;
    public string PendingReason { get; set; } = null!;
    public string ResponseCode { get; set; } = null!;
    public string ErrorCode { get; set; } = null!;
    public string ResponseMessage { get; set; } = null!;
    public string TransactionDate { get; set; } = null!;
    public string TransactionTime { get; set; } = null!;
    public long OperationDate { get; set; }
    public Dictionary<string, string> ExtraParameters { get; set; } = null!;
}

public class Payer
{
    public string MerchantPayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string BillingAddress { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string ContactPhone { get; set; } = null!;
    public string DniNumber { get; set; } = null!;
    public string DniType { get; set; } = null!;
}

public class AdditionalValueDetail
{
    public decimal Value { get; set; } 
    public string Currency { get; set; } = null!;
}