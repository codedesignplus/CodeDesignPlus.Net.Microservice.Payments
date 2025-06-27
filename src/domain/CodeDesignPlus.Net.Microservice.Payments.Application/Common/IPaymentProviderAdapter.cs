using System;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common;

public record AdapterInitiationResult(bool Succeeded, string? ProviderTransactionId, string? RedirectionUrl, string ErrorMessage);
public record AdapterStatusResult(PaymentStatus Status, string ProviderTransactionId, string Message, string RawResponse);


public interface IPaymentProviderAdapter
{

    PaymentProvider Provider { get; }
    
    Task<AdapterInitiationResult> InitiatePaymentAsync(InitiatePaymentCommand command, CancellationToken cancellationToken);
    Task<AdapterStatusResult> CheckStatusAsync(string referenceCode, CancellationToken cancellationToken);

}
