using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

/// <summary>
/// The collection account a tenant connects with its payment provider: the provider's merchant is the tenant's own,
/// and this service only keeps what it needs to charge and pay on its behalf.
/// </summary>
/// <remarks>
/// The secrets (private key, events and integrity secrets, payouts credentials) never live here: they are in the Vault
/// and the aggregate only keeps the reference and the last four characters of each, so the tenant can tell which keys
/// are connected without the platform ever showing them again.
/// <para>
/// It is born <see cref="MerchantAccountStatus.PendingConnection"/> when the tenant is created, with an id derived from
/// the tenant, so a redelivered event or a concurrent connection find the same document. The consumer that creates it
/// and the commands that connect it write the same document, hence <see cref="IVersionedAggregate"/>.
/// </para>
/// </remarks>
public class MerchantAccountAggregate(Guid id) : AggregateRootBase(id), IVersionedAggregate
{
    /// <summary>
    /// Namespace of the ids derived from the tenant. Changing it would orphan every stored account.
    /// </summary>
    private static readonly Guid IdNamespace = Guid.Parse("6b1f3c2e-0d4a-4f7e-9c58-3a2d7e914b60");

    /// <inheritdoc/>
    public long Version { get; private set; }

    /// <summary>
    /// Tenant that owns the account.
    /// </summary>
    public Guid Tenant { get; private set; }

    /// <summary>
    /// ISO 3166-1 alpha-2 country of the tenant, which decides the provider.
    /// </summary>
    public string Country { get; private set; } = null!;

    /// <summary>
    /// Code of the provider routed for the tenant's country, or null when no provider serves that country.
    /// </summary>
    public string? Provider { get; private set; }

    /// <summary>
    /// Current state of the connection.
    /// </summary>
    public MerchantAccountStatus Status { get; private set; }

    /// <summary>
    /// Provider environment of the connected keys.
    /// </summary>
    public MerchantAccountEnvironment? Environment { get; private set; }

    /// <summary>
    /// The merchant's public key.
    /// </summary>
    public string? PublicKey { get; private set; }

    /// <summary>
    /// Name the provider has for the merchant.
    /// </summary>
    public string? MerchantName { get; private set; }

    /// <summary>
    /// Where the secrets live in the Vault.
    /// </summary>
    public string? SecretReference { get; private set; }

    /// <summary>
    /// Last four characters of the private key.
    /// </summary>
    public string? PrivateKeyHint { get; private set; }

    /// <summary>
    /// Last four characters of the events secret.
    /// </summary>
    public string? EventsSecretHint { get; private set; }

    /// <summary>
    /// Last four characters of the integrity secret.
    /// </summary>
    public string? IntegritySecretHint { get; private set; }

    /// <summary>
    /// Provider account the payouts are paid from; null while payouts are not connected.
    /// </summary>
    public string? PayoutsSourceAccountId { get; private set; }

    /// <summary>
    /// Last four characters of the payouts API key.
    /// </summary>
    public string? PayoutsApiKeyHint { get; private set; }

    /// <summary>
    /// When the keys were last validated against the provider.
    /// </summary>
    public Instant? LastValidatedAt { get; private set; }

    /// <summary>
    /// Whether the account can pay third parties.
    /// </summary>
    public bool PayoutsEnabled => Status == MerchantAccountStatus.Active && !string.IsNullOrEmpty(PayoutsSourceAccountId);

    /// <summary>
    /// The id of the account of a tenant: always the same for the same tenant.
    /// </summary>
    /// <param name="tenant">The tenant that owns the account.</param>
    /// <returns>The deterministic id of the tenant's account.</returns>
    public static Guid IdFor(Guid tenant) => DeterministicGuid.Create(IdNamespace, tenant.ToString());

    /// <summary>
    /// Creates the account of a new tenant, waiting for its keys.
    /// </summary>
    /// <param name="tenant">The tenant that owns the account.</param>
    /// <param name="country">ISO 3166-1 alpha-2 country of the tenant.</param>
    /// <param name="provider">Provider routed for the country, or null when none serves it.</param>
    /// <param name="createdBy">Who created the tenant.</param>
    /// <returns>The account in <see cref="MerchantAccountStatus.PendingConnection"/>.</returns>
    public static MerchantAccountAggregate CreatePending(Guid tenant, string country, string? provider, Guid createdBy)
    {
        DomainGuard.GuidIsEmpty(tenant, Errors.MerchantAccountTenantCannotBeEmpty);
        DomainGuard.IsNullOrEmpty(country, Errors.CountryCannotBeNullOrEmpty);

        var id = IdFor(tenant);

        var aggregate = new MerchantAccountAggregate(id)
        {
            Tenant = tenant,
            Country = country,
            Provider = string.IsNullOrWhiteSpace(provider) ? null : provider,
            Status = MerchantAccountStatus.PendingConnection,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        aggregate.AddEvent(MerchantAccountCreatedDomainEvent.Create(id, tenant, country, aggregate.Provider));

        return aggregate;
    }

    /// <summary>
    /// Connects keys already validated against the provider to an account that had none.
    /// </summary>
    /// <param name="connection">What is kept of the validated keys.</param>
    /// <param name="updatedBy">Who connected the account.</param>
    public void Connect(MerchantConnection connection, Guid updatedBy)
    {
        DomainGuard.IsTrue(Status is not (MerchantAccountStatus.PendingConnection or MerchantAccountStatus.Disabled), Errors.MerchantAccountIsAlreadyConnected);

        Apply(connection, updatedBy);

        AddEvent(MerchantAccountConnectedDomainEvent.Create(Id, Tenant, Provider!, connection.Environment, PayoutsEnabled));
    }

    /// <summary>
    /// Replaces the keys of a connected account, after the tenant rotated them on the provider's panel.
    /// </summary>
    /// <param name="connection">What is kept of the validated keys.</param>
    /// <param name="updatedBy">Who replaced the keys.</param>
    public void ReplaceKeys(MerchantConnection connection, Guid updatedBy)
    {
        DomainGuard.IsTrue(Status is not (MerchantAccountStatus.Active or MerchantAccountStatus.Invalid), Errors.MerchantAccountIsNotConnected);

        Apply(connection, updatedBy);

        AddEvent(MerchantAccountKeysReplacedDomainEvent.Create(Id, Tenant, Provider!, connection.Environment, PayoutsEnabled));
    }

    /// <summary>
    /// Disconnects the account. Its secrets must already be deleted from the Vault.
    /// </summary>
    /// <param name="updatedBy">Who disconnected the account.</param>
    public void Disconnect(Guid updatedBy)
    {
        DomainGuard.IsTrue(Status is not (MerchantAccountStatus.Active or MerchantAccountStatus.Invalid), Errors.MerchantAccountIsNotConnected);

        Status = MerchantAccountStatus.Disabled;
        Environment = null;
        PublicKey = null;
        MerchantName = null;
        SecretReference = null;
        PrivateKeyHint = null;
        EventsSecretHint = null;
        IntegritySecretHint = null;
        PayoutsSourceAccountId = null;
        PayoutsApiKeyHint = null;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(MerchantAccountDisconnectedDomainEvent.Create(Id, Tenant));
    }

    /// <summary>
    /// Routes the account to a provider when it was created for a country no provider served yet.
    /// </summary>
    /// <param name="provider">The provider now routed for the account's country.</param>
    public void AssignProvider(string provider)
    {
        DomainGuard.IsNullOrEmpty(provider, Errors.PaymentProviderIsRequired);

        Provider = provider;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    private void Apply(MerchantConnection connection, Guid updatedBy)
    {
        DomainGuard.IsNull(connection, Errors.MerchantConnectionCannotBeNull);
        DomainGuard.IsNullOrEmpty(Provider, Errors.PaymentProviderIsRequired);
        DomainGuard.IsNullOrEmpty(connection.PublicKey, Errors.MerchantPublicKeyIsRequired);
        DomainGuard.IsNullOrEmpty(connection.SecretReference, Errors.MerchantSecretReferenceIsRequired);

        var now = SystemClock.Instance.GetCurrentInstant();

        Status = MerchantAccountStatus.Active;
        Environment = connection.Environment;
        PublicKey = connection.PublicKey;
        MerchantName = connection.MerchantName;
        SecretReference = connection.SecretReference;
        PrivateKeyHint = connection.PrivateKeyHint;
        EventsSecretHint = connection.EventsSecretHint;
        IntegritySecretHint = connection.IntegritySecretHint;
        PayoutsSourceAccountId = connection.PayoutsSourceAccountId;
        PayoutsApiKeyHint = connection.PayoutsApiKeyHint;
        LastValidatedAt = now;
        UpdatedBy = updatedBy;
        UpdatedAt = now;
    }
}
