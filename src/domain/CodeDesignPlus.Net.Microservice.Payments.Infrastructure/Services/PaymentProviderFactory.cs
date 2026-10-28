using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Exceptions.Extensions;
using AppErrors = CodeDesignPlus.Net.Microservice.Payments.Application.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;

/// <summary>
/// Finds a provider's adapter by its code among the registered ones. Adding a provider is registering its adapter;
/// nothing here changes.
/// </summary>
public sealed class PaymentProviderFactory(IEnumerable<ICollectProvider> collectProviders, IEnumerable<IMerchantCredentialsProvider> credentialsProviders, IEnumerable<IPayoutProvider> payoutProviders) : IPaymentProviderFactory
{
    /// <inheritdoc/>
    public ICollectProvider GetCollect(string provider) => Find(collectProviders, provider, AppErrors.PaymentProviderNotSupported);

    /// <inheritdoc/>
    public IMerchantCredentialsProvider GetCredentials(string provider) => Find(credentialsProviders, provider, AppErrors.PaymentProviderNotSupported);

    /// <inheritdoc/>
    public IPayoutProvider GetPayouts(string provider) => Find(payoutProviders, provider, AppErrors.PaymentProviderCannotPayThirdParties);

    private static T Find<T>(IEnumerable<T> providers, string provider, Error error) where T : IPaymentProvider
    {
        var found = providers.FirstOrDefault(x => string.Equals(x.Code, provider, StringComparison.OrdinalIgnoreCase));

        if (found is null)
        {
            throw new CodeDesignPlusException(Layer.Application, error.GetCode(), error.GetMessage());
        }

        return found;
    }
}
