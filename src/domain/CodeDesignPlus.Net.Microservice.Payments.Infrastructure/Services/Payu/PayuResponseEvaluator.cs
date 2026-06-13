using System.Text.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

public static class PayuResponseEvaluator
{
    public static bool IsRetryableError(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("code", out var codeProp) &&
                codeProp.ValueKind == JsonValueKind.String &&
                string.Equals(codeProp.GetString(), "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
