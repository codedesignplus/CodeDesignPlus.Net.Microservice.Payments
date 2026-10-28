using CodeDesignPlus.Net.Vault.Abstractions.Options;
using CodeDesignPlus.Net.Vault.Services;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Vault;

/// <summary>
/// Keeps the secrets of each merchant account in the Vault's key-value engine, one secret per account.
/// </summary>
/// <remarks>
/// They live in the same mount the service reads its configuration from, under
/// <c>{AppName}/merchant-accounts/{account}</c>: a path of its own, so the configuration provider, which reads only
/// <c>{AppName}</c>, never loads them. Deleting removes the metadata, which destroys every version: after a tenant
/// disconnects or is purged, no earlier version of its keys can be read back.
/// <para>
/// The SDK exposes the authenticated client through <see cref="VaultClientProvider"/>, which also renews its token; the
/// store uses it instead of opening a second session.
/// </para>
/// </remarks>
public sealed class VaultMerchantSecretStore(VaultClientProvider clientProvider, IOptions<VaultOptions> options) : IMerchantSecretStore
{
    private const string PrivateKeyField = "privateKey";
    private const string EventsSecretField = "eventsSecret";
    private const string IntegritySecretField = "integritySecret";
    private const string PayoutsPrincipalIdField = "payoutsPrincipalId";
    private const string PayoutsApiKeyField = "payoutsApiKey";

    private string MountPoint => $"{options.Value.Solution}-keyvalue";

    /// <inheritdoc/>
    public async Task<string> SaveAsync(Guid accountId, MerchantSecrets secrets, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(secrets);

        var path = PathOf(accountId);

        var data = new Dictionary<string, object>
        {
            [PrivateKeyField] = secrets.PrivateKey,
            [EventsSecretField] = secrets.EventsSecret,
            [IntegritySecretField] = secrets.IntegritySecret
        };

        if (secrets.HasPayouts)
        {
            data[PayoutsPrincipalIdField] = secrets.PayoutsPrincipalId!;
            data[PayoutsApiKeyField] = secrets.PayoutsApiKey!;
        }

        await clientProvider.Client.V1.Secrets.KeyValue.V2.WriteSecretAsync(path, data, mountPoint: MountPoint);

        return path;
    }

    /// <inheritdoc/>
    public async Task<MerchantSecrets?> GetAsync(string reference, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);

        try
        {
            var secret = await clientProvider.Client.V1.Secrets.KeyValue.V2.ReadSecretAsync(reference, mountPoint: MountPoint);

            var data = secret.Data.Data;

            return new MerchantSecrets(Read(data, PrivateKeyField)!, Read(data, EventsSecretField)!, Read(data, IntegritySecretField)!, Read(data, PayoutsPrincipalIdField), Read(data, PayoutsApiKeyField));
        }
        catch (VaultSharp.Core.VaultApiException exception) when (exception.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid accountId, CancellationToken cancellationToken)
    {
        try
        {
            await clientProvider.Client.V1.Secrets.KeyValue.V2.DeleteMetadataAsync(PathOf(accountId), mountPoint: MountPoint);
        }
        catch (VaultSharp.Core.VaultApiException exception) when (exception.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Already gone: deleting what is not there is harmless.
        }
    }

    private string PathOf(Guid accountId) => $"{options.Value.AppName}/merchant-accounts/{accountId}";

    private static string? Read(IDictionary<string, object> data, string field) => data.TryGetValue(field, out var value) ? value?.ToString() : null;
}
