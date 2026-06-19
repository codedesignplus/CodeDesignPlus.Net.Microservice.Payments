using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common;

public interface IPaymentProviderAdapterFactory
{
    IPaymentProviderAdapter GetAdapter(PaymentProvider provider);
    Task<AdapterResolutionResult> GetAdapterAsync(PaymentProvider provider, Guid tenantId, CancellationToken cancellationToken);
}

public class AdapterResolutionResult
{
    public required IPaymentProviderAdapter Adapter { get; init; }
    public PaymentProviderConfigAggregate? Config { get; init; }
    public bool UsesTenantCredentials => Config is not null;
}
