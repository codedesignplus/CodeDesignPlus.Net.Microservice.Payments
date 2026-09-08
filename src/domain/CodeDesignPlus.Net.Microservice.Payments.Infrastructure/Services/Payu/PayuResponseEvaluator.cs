using System.Text.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

/// <summary>
/// Reads a PayU response body to decide what to do with it.
/// </summary>
public static class PayuResponseEvaluator
{
    /// <summary>
    /// Whether a failed PayU response is worth another attempt.
    /// </summary>
    /// <remarks>
    /// This used to return <c>true</c> for <b>any</b> <c>code=ERROR</c>, ignoring the <c>error</c> field
    /// PayU sends alongside it. A card with an expired date — a permanent validation error that could never
    /// succeed — was retried seven times over roughly 26 seconds before failing.
    /// <para>
    /// The rule deliberately avoids a hardcoded list of PayU codes: that list is PayU's, not ours, and it
    /// would age without us noticing (see rules/26). Instead it reads intent — <b>when PayU explains what is
    /// wrong, the request is wrong</b>, and retrying an unchanged request cannot fix it. Only an opaque
    /// failure, one PayU did not explain, may be transient and earns another attempt.
    /// </para>
    /// </remarks>
    /// <param name="responseBody">The raw response body returned by PayU.</param>
    /// <returns><c>true</c> only for an unexplained error.</returns>
    public static bool IsRetryableError(string responseBody)
    {
        if (!IsError(responseBody))
            return false;

        return string.IsNullOrWhiteSpace(GetErrorMessage(responseBody));
    }

    /// <summary>
    /// The reason PayU gave for rejecting the request, if it gave one.
    /// </summary>
    /// <remarks>
    /// It exists so the reason can reach the caller. PayU does send it, but it used to travel only in the
    /// second argument of the exception and never reached the ProblemDetails: whoever called the API saw
    /// nothing beyond "PayU returned code=ERROR", with no way to tell whether to retry, fix the card or
    /// call support.
    /// </remarks>
    /// <param name="responseBody">The raw response body returned by PayU.</param>
    /// <returns>The message, or <c>null</c> when PayU did not explain itself or the body is not JSON.</returns>
    public static string? GetErrorMessage(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty("error", out var errorProp) &&
                errorProp.ValueKind == JsonValueKind.String)
            {
                var message = errorProp.GetString();

                return string.IsNullOrWhiteSpace(message) ? null : message;
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Whether PayU rejected the request outright, as opposed to processing it and returning an outcome.
    /// </summary>
    private static bool IsError(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);

            return document.RootElement.TryGetProperty("code", out var codeProp) &&
                   codeProp.ValueKind == JsonValueKind.String &&
                   string.Equals(codeProp.GetString(), "ERROR", StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            // A body we cannot parse — an HTML error page, say. Retrying on a guess is worse than failing.
            return false;
        }
    }
}
