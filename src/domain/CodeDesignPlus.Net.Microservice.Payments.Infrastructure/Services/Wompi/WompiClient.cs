using System.Net;
using System.Net.Http.Headers;
using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Exceptions.Extensions;
using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Logging;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi.Models;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;

/// <summary>
/// The calls this service makes to Wompi's APIs.
/// </summary>
public interface IWompiClient
{
    /// <summary>
    /// The merchant a public key belongs to.
    /// </summary>
    /// <param name="publicKey">The merchant's public key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The merchant, or null when Wompi does not recognize the key.</returns>
    Task<WompiMerchant?> GetMerchantAsync(string publicKey, CancellationToken cancellationToken);

    /// <summary>
    /// Whether Wompi accepts a private key, asking for the transactions of a reference nobody used.
    /// </summary>
    /// <param name="privateKey">The merchant's private key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Whether the key was accepted.</returns>
    Task<bool> IsPrivateKeyAcceptedAsync(string privateKey, CancellationToken cancellationToken);

    /// <summary>
    /// The transactions of a reference.
    /// </summary>
    /// <param name="privateKey">The private key of the merchant that collected them.</param>
    /// <param name="reference">The reference: the id of the payment in this service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transactions, possibly none.</returns>
    Task<IReadOnlyList<WompiTransaction>> GetTransactionsByReferenceAsync(string privateKey, string reference, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a payouts batch.
    /// </summary>
    /// <param name="credentials">The payouts credentials of the merchant that pays.</param>
    /// <param name="request">The batch.</param>
    /// <param name="idempotencyKey">Repeating the call with the same key does not pay twice.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The batch as Wompi registered it.</returns>
    Task<WompiPayoutBatch> CreatePayoutAsync(MerchantCredentials credentials, WompiPayoutRequest request, string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>
    /// The banks Wompi can pay to.
    /// </summary>
    /// <param name="credentials">The payouts credentials of a merchant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The banks.</returns>
    Task<IReadOnlyList<WompiBank>> GetBanksAsync(MerchantCredentials credentials, CancellationToken cancellationToken);
}

/// <summary>
/// Reaches Wompi's APIs. The base URL follows the environment of the key, so one client serves sandbox and production
/// merchants at once.
/// </summary>
/// <remarks>
/// Keys travel only in headers and are never logged: a log line records the method, the path without query and the
/// status. A payouts body is logged through <see cref="ProviderLogPayload"/>, which leaves out what is marked
/// <see cref="NotLoggedAttribute"/>.
/// </remarks>
public sealed class WompiClient(IHttpClientFactory httpClientFactory, IOptions<WompiOptions> options, ILogger<WompiClient> logger) : IWompiClient
{
    /// <summary>
    /// Name of the HTTP client registered for Wompi.
    /// </summary>
    public const string HttpClientName = "Wompi";

    /// <inheritdoc/>
    public async Task<WompiMerchant?> GetMerchantAsync(string publicKey, CancellationToken cancellationToken)
    {
        var environment = WompiKeys.EnvironmentOf(publicKey);

        if (environment is null)
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, Collect(environment.Value, $"merchants/{Uri.EscapeDataString(publicKey)}"));

        using var response = await SendAsync(request, cancellationToken);

        if (IsRejection(response.StatusCode))
        {
            return null;
        }

        await EnsureAvailableAsync(response, cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<WompiResponse<WompiMerchant>>(body)?.Data;
    }

    /// <inheritdoc/>
    public async Task<bool> IsPrivateKeyAcceptedAsync(string privateKey, CancellationToken cancellationToken)
    {
        var environment = WompiKeys.EnvironmentOf(privateKey);

        if (environment is null)
        {
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, Collect(environment.Value, $"transactions?reference={Guid.NewGuid()}"));

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", privateKey);

        using var response = await SendAsync(request, cancellationToken);

        if (IsRejection(response.StatusCode))
        {
            return false;
        }

        await EnsureAvailableAsync(response, cancellationToken);

        return true;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WompiTransaction>> GetTransactionsByReferenceAsync(string privateKey, string reference, CancellationToken cancellationToken)
    {
        var environment = WompiKeys.EnvironmentOf(privateKey);

        InfrastructureGuard.IsNull(environment, Errors.PaymentProviderKeyIsNotRecognized);

        using var request = new HttpRequestMessage(HttpMethod.Get, Collect(environment!.Value, $"transactions?reference={Uri.EscapeDataString(reference)}"));

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", privateKey);

        using var response = await SendAsync(request, cancellationToken);

        await EnsureAvailableAsync(response, cancellationToken);
        EnsureKeysAccepted(response);

        InfrastructureGuard.IsFalse(response.IsSuccessStatusCode, Errors.PaymentGatewayRejectedTheRequest);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<WompiResponse<List<WompiTransaction>>>(body)?.Data ?? [];
    }

    /// <inheritdoc/>
    public async Task<WompiPayoutBatch> CreatePayoutAsync(MerchantCredentials credentials, WompiPayoutRequest request, string idempotencyKey, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending payouts batch {Reference} to Wompi: {Batch}", request.Reference, ProviderLogPayload.Serialize(request));

        using var message = new HttpRequestMessage(HttpMethod.Post, Payouts(credentials, "payouts"))
        {
            Content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json")
        };

        AddPayoutsHeaders(message, credentials);
        message.Headers.Add("idempotency-key", idempotencyKey);

        using var response = await SendAsync(message, cancellationToken);

        await EnsureAvailableAsync(response, cancellationToken);
        EnsureKeysAccepted(response);

        InfrastructureGuard.IsFalse(response.IsSuccessStatusCode, Errors.PaymentGatewayRejectedTheRequest);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        var batch = JsonSerializer.Deserialize<WompiResponse<WompiPayoutBatch>>(body)?.Data;

        InfrastructureGuard.IsNull(batch, Errors.PaymentGatewayRejectedTheRequest);

        return batch!;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WompiBank>> GetBanksAsync(MerchantCredentials credentials, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, Payouts(credentials, "banks"));

        AddPayoutsHeaders(message, credentials);

        using var response = await SendAsync(message, cancellationToken);

        await EnsureAvailableAsync(response, cancellationToken);

        InfrastructureGuard.IsFalse(response.IsSuccessStatusCode, Errors.PaymentGatewayRejectedTheRequest);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<WompiResponse<List<WompiBank>>>(body)?.Data ?? [];
    }

    private Uri Collect(MerchantAccountEnvironment environment, string path)
    {
        var root = environment == MerchantAccountEnvironment.Production ? options.Value.ProductionUrl : options.Value.SandboxUrl;

        return Combine(root, path);
    }

    private Uri Payouts(MerchantCredentials credentials, string path)
    {
        var root = credentials.Environment == MerchantAccountEnvironment.Production ? options.Value.PayoutsProductionUrl : options.Value.PayoutsSandboxUrl;

        return Combine(root, path);
    }

    private static Uri Combine(Uri root, string path) => new($"{root.ToString().TrimEnd('/')}/{path}");

    private static void AddPayoutsHeaders(HttpRequestMessage message, MerchantCredentials credentials)
    {
        InfrastructureGuard.IsFalse(credentials.Secrets.HasPayouts, Errors.PaymentProviderKeyIsNotRecognized);

        message.Headers.Add("user-principal-id", credentials.Secrets.PayoutsPrincipalId);
        message.Headers.Add("x-api-key", credentials.Secrets.PayoutsApiKey);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);

        var response = await client.SendAsync(request, cancellationToken);

        logger.LogInformation("Wompi {Method} {Path} answered {Status}", request.Method, request.RequestUri?.AbsolutePath, (int)response.StatusCode);

        return response;
    }

    // Wompi answers 401 to a key it does not accept and 404 or 422 to a public key it does not know.
    private static bool IsRejection(HttpStatusCode status) => status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound or HttpStatusCode.UnprocessableEntity;

    // A rejected key is told apart from any other refusal: repeated, it invalidates the collection account.
    private static void EnsureKeysAccepted(HttpResponseMessage response)
    {
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            var error = Application.Errors.MerchantKeysRejectedByProvider;

            throw new CodeDesignPlusException(Layer.Infrastructure, error.GetCode(), error.GetMessage());
        }
    }

    private async Task EnsureAvailableAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if ((int)response.StatusCode < 500)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        logger.LogWarning("Wompi is not available: {Status} {Body}", (int)response.StatusCode, body.Length > 500 ? body[..500] : body);

        throw new CodeDesignPlusException(Layer.Infrastructure, Errors.PaymentProviderIsUnavailable.GetCode(), Errors.PaymentProviderIsUnavailable.GetMessage());
    }
}
