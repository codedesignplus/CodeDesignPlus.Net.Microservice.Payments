namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;

/// <summary>
/// Validates the keys a tenant registers before they are stored.
/// </summary>
public interface IMerchantCredentialsProvider : IPaymentProvider
{
    /// <summary>
    /// Checks the keys against the provider: the public key must name a merchant and the private key must be accepted.
    /// </summary>
    /// <param name="publicKey">The merchant's public key.</param>
    /// <param name="secrets">The merchant's secrets.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Whether the keys work, their environment and the merchant's name.</returns>
    Task<MerchantValidation> ValidateAsync(string publicKey, MerchantSecrets secrets, CancellationToken cancellationToken);
}

/// <summary>
/// The result of validating merchant keys against the provider.
/// </summary>
/// <param name="IsValid">Whether the provider accepted the keys.</param>
/// <param name="Environment">Environment the keys belong to, when it could be told.</param>
/// <param name="MerchantName">Name the provider has for the merchant.</param>
/// <param name="Reason">Why the keys were rejected, for the log; never contains a secret.</param>
public sealed record MerchantValidation(bool IsValid, MerchantAccountEnvironment? Environment, string? MerchantName, string? Reason);
