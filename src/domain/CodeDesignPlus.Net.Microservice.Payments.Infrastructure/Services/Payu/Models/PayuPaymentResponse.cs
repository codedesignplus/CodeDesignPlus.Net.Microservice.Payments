namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

public class PayuPaymentResponse
{
    public string Code { get; set; } = null!;
    public string Error { get; set; } = null!;
    public PaymentTransactionResponse TransactionResponse { get; set; } = null!;

    public Dictionary<string, string> ConvertToProviderResponse()
    {
        var transactionResponse = this.TransactionResponse?.ConvertToProviderResponse() ?? [];

        var providerResponse = new Dictionary<string, string>
        {
            {"Code", this.Code},
            {"Error", this.Error}
        };

        foreach (var kvp in transactionResponse)
        {
            providerResponse[$"Transaction:{kvp.Key}"] = kvp.Value;
        }

        return providerResponse;
    }
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

    public Dictionary<string, string> ConvertToProviderResponse()
    {
        var additionalInfo = this.AdditionalInfo?.ConvertToProviderResponse() ?? [];

        var providerResponse = new Dictionary<string, string>
        {
            {"OrderId", this.OrderId.ToString()},
            {"TransactionId", this.TransactionId},
            {"State", this.State},
            {"PaymentNetworkResponseCode", this.PaymentNetworkResponseCode},
            {"PaymentNetworkResponseErrorMessage", this.PaymentNetworkResponseErrorMessage},
            {"TrazabilityCode", this.TrazabilityCode},
            {"AuthorizationCode", this.AuthorizationCode},
            {"PendingReason", this.PendingReason},
            {"ResponseCode", this.ResponseCode},
            {"ErrorCode", this.ErrorCode},
            {"ResponseMessage", this.ResponseMessage},
            {"TransactionDate", this.TransactionDate},
            {"TransactionTime", this.TransactionTime},
            {"OperationDate", this.OperationDate.ToString()},
            {"ReferenceQuestionnaire", this.ReferenceQuestionnaire}
        };

        foreach (var kvp in this.ExtraParameters)
        {
            providerResponse[$"ExtraParameter:{kvp.Key}"] = kvp.Value;
        }

        foreach (var kvp in additionalInfo)
        {
            providerResponse[$"AdditionalInfo:{kvp.Key}"] = kvp.Value;
        }

        return providerResponse;
    }
}

public class AdditionalInfo
{
    public string PaymentNetwork { get; set; } = null!;
    public string RejectionType { get; set; } = null!;
    public string ResponseNetworkMessage { get; set; } = null!;
    public string TravelAgencyAuthorizationCode { get; set; } = null!;
    public string CardType { get; set; } = null!;
    public string TransactionType { get; set; } = null!;

    public Dictionary<string, string> ConvertToProviderResponse()
    {
        return new Dictionary<string, string>
        {
            {"PaymentNetwork", this.PaymentNetwork},
            {"RejectionType", this.RejectionType},
            {"ResponseNetworkMessage", this.ResponseNetworkMessage},
            {"TravelAgencyAuthorizationCode", this.TravelAgencyAuthorizationCode},
            {"CardType", this.CardType},
            {"TransactionType", this.TransactionType}
        };
    }
}