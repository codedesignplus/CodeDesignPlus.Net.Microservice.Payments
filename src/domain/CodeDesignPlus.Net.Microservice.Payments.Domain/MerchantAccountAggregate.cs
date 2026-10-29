using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

/// <summary>
/// The collection account a tenant connects with its payment provider: the provider's merchant is the tenant's own,
/// and this service only keeps what it needs to charge and pay on its behalf.
/// </summary>
/// <remarks>
/// The secrets (private key, events and integrity secrets, payouts credentials) are never stored in clear: the aggregate
/// keeps only their ciphertext from the Vault's Transit engine, bound to the tenant, and the last four characters of
/// each, so the tenant can tell which keys are connected without the platform ever showing them again.
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
    /// The secrets, encrypted by the Vault; null while the account has no keys.
    /// </summary>
    public SealedSecrets? Secrets { get; private set; }

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
    /// How many times in a row the provider rejected the keys since they last worked. One rejection may be transient;
    /// the account is invalidated only when they repeat.
    /// </summary>
    public int ConsecutiveRejections { get; private set; }

    /// <summary>
    /// Code of the error catalog that says why the provider stopped accepting the keys, while the account is
    /// <see cref="MerchantAccountStatus.Invalid"/>. A code and not a text, so each reader shows it in its language.
    /// </summary>
    public string? InvalidReasonCode { get; private set; }

    /// <summary>
    /// The rates the provider charges the tenant; null while the tenant has not set them, and then the provider's
    /// default of the deployment applies (<c>PaymentProviders:DefaultFees</c>).
    /// </summary>
    public ProviderFeeSchedule? FeeSchedule { get; private set; }

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
    /// Disconnects the account and forgets the ciphertext of its secrets.
    /// </summary>
    /// <param name="updatedBy">Who disconnected the account.</param>
    public void Disconnect(Guid updatedBy)
    {
        DomainGuard.IsTrue(Status is not (MerchantAccountStatus.Active or MerchantAccountStatus.Invalid), Errors.MerchantAccountIsNotConnected);

        Status = MerchantAccountStatus.Disabled;
        InvalidReasonCode = null;
        ConsecutiveRejections = 0;
        Environment = null;
        PublicKey = null;
        MerchantName = null;
        Secrets = null;
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
    /// Counts a rejection of the keys by the provider and invalidates the account when they repeat.
    /// </summary>
    /// <param name="reasonCode">Code of the error catalog that says what the provider rejected.</param>
    /// <param name="threshold">How many rejections in a row invalidate the account.</param>
    /// <returns>Whether the account changed and must be saved.</returns>
    public bool RegisterRejection(string reasonCode, int threshold)
    {
        DomainGuard.IsNullOrEmpty(reasonCode, Errors.FailureReasonIsRequired);

        if (Status != MerchantAccountStatus.Active)
        {
            return false;
        }

        ConsecutiveRejections++;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        if (ConsecutiveRejections >= threshold)
        {
            Invalidate(reasonCode, null);
        }

        return true;
    }

    /// <summary>
    /// The provider accepted the keys again: earlier rejections were transient.
    /// </summary>
    /// <returns>Whether the account changed and must be saved.</returns>
    public bool RegisterAcceptance()
    {
        if (ConsecutiveRejections == 0)
        {
            return false;
        }

        ConsecutiveRejections = 0;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        return true;
    }

    /// <summary>
    /// The provider no longer accepts the keys: the tenant stops charging online until it replaces them.
    /// </summary>
    /// <param name="reasonCode">Code of the error catalog that says what the provider rejected.</param>
    /// <param name="updatedBy">Who checked the keys, or null when the platform noticed it by itself.</param>
    public void Invalidate(string reasonCode, Guid? updatedBy)
    {
        DomainGuard.IsNullOrEmpty(reasonCode, Errors.FailureReasonIsRequired);
        DomainGuard.IsTrue(Status is not (MerchantAccountStatus.Active or MerchantAccountStatus.Invalid), Errors.MerchantAccountIsNotConnected);

        var wasActive = Status == MerchantAccountStatus.Active;

        Status = MerchantAccountStatus.Invalid;
        InvalidReasonCode = reasonCode;
        ConsecutiveRejections = 0;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        if (wasActive)
        {
            AddEvent(MerchantAccountInvalidatedDomainEvent.Create(Id, Tenant, Provider!, reasonCode));
        }
    }

    /// <summary>
    /// The stored keys were checked again against the provider and work.
    /// </summary>
    /// <param name="updatedBy">Who checked them.</param>
    public void ConfirmKeys(Guid updatedBy)
    {
        DomainGuard.IsTrue(Status is not (MerchantAccountStatus.Active or MerchantAccountStatus.Invalid), Errors.MerchantAccountIsNotConnected);

        var wasInvalid = Status == MerchantAccountStatus.Invalid;
        var now = SystemClock.Instance.GetCurrentInstant();

        Status = MerchantAccountStatus.Active;
        InvalidReasonCode = null;
        ConsecutiveRejections = 0;
        LastValidatedAt = now;
        UpdatedBy = updatedBy;
        UpdatedAt = now;

        if (wasInvalid)
        {
            AddEvent(MerchantAccountRevalidatedDomainEvent.Create(Id, Tenant, Provider!));
        }
    }

    /// <summary>
    /// Sets the rates the provider charges the tenant, as its contract with the provider says. Applies to the payments
    /// that succeed from now on; the ones already closed keep the commission they were published with.
    /// </summary>
    /// <param name="schedule">The rates.</param>
    /// <param name="updatedBy">Who set them.</param>
    public void SetFeeSchedule(ProviderFeeSchedule schedule, Guid updatedBy)
    {
        DomainGuard.IsNull(schedule, Errors.ProviderFeeScheduleIsRequired);

        FeeSchedule = schedule;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(MerchantAccountFeeScheduleChangedDomainEvent.Create(Id, Tenant, schedule));
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
        DomainGuard.IsNull(connection.Secrets, Errors.MerchantSecretsAreRequired);

        var now = SystemClock.Instance.GetCurrentInstant();

        Status = MerchantAccountStatus.Active;
        InvalidReasonCode = null;
        ConsecutiveRejections = 0;
        Environment = connection.Environment;
        PublicKey = connection.PublicKey;
        MerchantName = connection.MerchantName;
        Secrets = connection.Secrets;
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
