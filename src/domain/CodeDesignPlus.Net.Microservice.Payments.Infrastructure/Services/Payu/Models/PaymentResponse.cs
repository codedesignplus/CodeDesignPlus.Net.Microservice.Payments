namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class PaymentResponse
{
    public string Code { get; set; } = null!;
    public object Error { get; set; } = null!;
    public PaymentTransactionResponse TransactionResponse { get; set; } = null!;
}

public class PaymentTransactionResponse
{
    public long OrderId { get; set; }
    public string TransactionId { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PaymentNetworkResponseCode { get; set; } = null!;
    public object PaymentNetworkResponseErrorMessage { get; set; } = null!;
    public string TrazabilityCode { get; set; } = null!;
    public string AuthorizationCode { get; set; } = null!;
    public string PendingReason { get; set; } = null!;
    public string ResponseCode { get; set; } = null!;
    public object ErrorCode { get; set; } = null!;
    public string ResponseMessage { get; set; } = null!;
    public object TransactionDate { get; set; } = null!;
    public object TransactionTime { get; set; } = null!;
    public long OperationDate { get; set; }
    public object ReferenceQuestionnaire { get; set; } = null!;
    public Dictionary<string, string> ExtraParameters { get; set; } = null!;
    public AdditionalInfo AdditionalInfo { get; set; } = null!;
}

public class AdditionalInfo
{
    public string PaymentNetwork { get; set; } = null!;
    public string RejectionType { get; set; } = null!;
    public object ResponseNetworkMessage { get; set; } = null!;
    public object TravelAgencyAuthorizationCode { get; set; } = null!;
    public string CardType { get; set; } = null!;
    public string TransactionType { get; set; } = null!;
}