using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common;


public interface IPaymentProviderAdapter
{
    PaymentProvider Provider { get; }
    Task<PaymentResponseDto> InitiatePaymentAsync(InitiatePaymentCommand command, CancellationToken cancellationToken);
    Task<PaymentResponseDto> CheckStatusAsync(string id, CancellationToken cancellationToken);
    Task<bool> CheckSignature(string merchantId, Guid referenceCode, string value, string currency, string state, string receivedSignature);
}