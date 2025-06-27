namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class PayuPaymentResponse
{
    public string Code { get; set; } = null!;
    public string Error { get; set; } = null!;
    public PaymentTransactionResponse TransactionResponse { get; set; } = null!;
}

public class PaymentTransactionResponse
{
    public long OrderId { get; set; }
    public string TransactionId { get; set; } = null!;
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
    public string ReferenceQuestionnaire { get; set; } = null!;
    public Dictionary<string, string> ExtraParameters { get; set; } = null!;
    public AdditionalInfo AdditionalInfo { get; set; } = null!;
}

public class AdditionalInfo
{
    public string PaymentNetwork { get; set; } = null!;
    public string RejectionType { get; set; } = null!;
    public string ResponseNetworkMessage { get; set; } = null!;
    public string TravelAgencyAuthorizationCode { get; set; } = null!;
    public string CardType { get; set; } = null!;
    public string TransactionType { get; set; } = null!;
}