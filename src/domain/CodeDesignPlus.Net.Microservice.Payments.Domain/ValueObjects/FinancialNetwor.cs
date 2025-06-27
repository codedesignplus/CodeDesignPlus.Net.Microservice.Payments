using System.Text.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public sealed record FinancialNetwork(
    string PaymentNetworkResponseCode,
    string PaymentNetworkResponseErrorMessage,
    string TrazabilityCode,
    string AuthorizationCode,
    string ResponseCode
);
