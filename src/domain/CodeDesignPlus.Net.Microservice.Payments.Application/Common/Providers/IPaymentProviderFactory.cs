namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;

/// <summary>
/// Finds the adapter of a provider by its code, one capability at a time.
/// </summary>
public interface IPaymentProviderFactory
{
    /// <summary>
    /// The collection capability of a provider.
    /// </summary>
    /// <param name="provider">The provider code.</param>
    /// <returns>The adapter that collects with that provider.</returns>
    /// <exception cref="Exceptions.CodeDesignPlusException">Thrown when no adapter serves the provider.</exception>
    ICollectProvider GetCollect(string provider);

    /// <summary>
    /// The capability that validates a merchant's keys.
    /// </summary>
    /// <param name="provider">The provider code.</param>
    /// <returns>The adapter that validates keys of that provider.</returns>
    /// <exception cref="Exceptions.CodeDesignPlusException">Thrown when no adapter serves the provider.</exception>
    IMerchantCredentialsProvider GetCredentials(string provider);

    /// <summary>
    /// The payouts capability of a provider.
    /// </summary>
    /// <param name="provider">The provider code.</param>
    /// <returns>The adapter that pays third parties with that provider.</returns>
    /// <exception cref="Exceptions.CodeDesignPlusException">Thrown when the provider cannot pay third parties.</exception>
    IPayoutProvider GetPayouts(string provider);
}
