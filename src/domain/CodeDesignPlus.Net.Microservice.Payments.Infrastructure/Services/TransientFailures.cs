using CodeDesignPlus.Net.Exceptions;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;

/// <summary>
/// The failures of a dependency that the caller should retry later: the Vault or the provider did not answer.
/// </summary>
/// <remarks>
/// The entrypoints answer them as unavailable (503 in REST, <c>Unavailable</c> in gRPC) instead of a bad request, so a
/// provider retries its event and a module knows its payment can be tried again.
/// </remarks>
public static class TransientFailures
{
    /// <summary>
    /// Whether the exception is a dependency that did not answer.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>True when retrying later can succeed.</returns>
    public static bool IsTransient(CodeDesignPlusException exception) => exception.Code == Errors.PaymentProviderIsUnavailable.Code || exception.Code == Errors.MerchantSecretsVaultIsUnavailable.Code;
}
