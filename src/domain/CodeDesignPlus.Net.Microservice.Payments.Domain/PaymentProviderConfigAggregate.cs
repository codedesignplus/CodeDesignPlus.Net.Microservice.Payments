namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class PaymentProviderConfigAggregate(Guid id) : AggregateRoot(id)
{
    public PaymentProvider Provider { get; private set; }
    public int AccountId { get; private set; }
    public string MerchantId { get; private set; } = null!;
    public string ApiKey { get; private set; } = null!;
    public string ApiLogin { get; private set; } = null!;
    public string SecretKey { get; private set; } = null!;
    public string Currency { get; private set; } = null!;
    public string PaymentCountry { get; private set; } = null!;
    public bool IsTest { get; private set; }
    public string NotificationUrl { get; private set; } = null!;
    public bool RequiresDisbursement { get; private set; }

    public static PaymentProviderConfigAggregate Create(
        Guid id,
        PaymentProvider provider,
        int accountId,
        string merchantId,
        string apiKey,
        string apiLogin,
        string secretKey,
        string currency,
        string paymentCountry,
        bool isTest,
        string notificationUrl,
        bool requiresDisbursement,
        Guid tenant,
        Guid createdBy)
    {
        DomainGuard.GuidIsEmpty(id, Errors.PaymentProviderConfigIdIsRequired);
        DomainGuard.IsNullOrEmpty(merchantId, Errors.PaymentProviderConfigMerchantIdIsRequired);
        DomainGuard.IsNullOrEmpty(apiKey, Errors.PaymentProviderConfigApiKeyIsRequired);
        DomainGuard.IsNullOrEmpty(apiLogin, Errors.PaymentProviderConfigApiLoginIsRequired);
        DomainGuard.IsNullOrEmpty(secretKey, Errors.PaymentProviderConfigSecretKeyIsRequired);
        DomainGuard.IsNullOrEmpty(currency, Errors.CurrencyIsRequired);
        DomainGuard.IsNullOrEmpty(notificationUrl, Errors.PaymentProviderConfigNotificationUrlIsRequired);

        return new PaymentProviderConfigAggregate(id)
        {
            Provider = provider,
            AccountId = accountId,
            MerchantId = merchantId,
            ApiKey = apiKey,
            ApiLogin = apiLogin,
            SecretKey = secretKey,
            Currency = currency,
            PaymentCountry = paymentCountry,
            IsTest = isTest,
            NotificationUrl = notificationUrl,
            RequiresDisbursement = requiresDisbursement,
            IsActive = true,
            Tenant = tenant,
            CreatedBy = createdBy,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };
    }

    public void Update(
        int accountId,
        string merchantId,
        string apiKey,
        string apiLogin,
        string secretKey,
        string currency,
        string paymentCountry,
        bool isTest,
        string notificationUrl,
        bool requiresDisbursement,
        bool isActive,
        Guid updatedBy)
    {
        DomainGuard.IsNullOrEmpty(merchantId, Errors.PaymentProviderConfigMerchantIdIsRequired);
        DomainGuard.IsNullOrEmpty(apiKey, Errors.PaymentProviderConfigApiKeyIsRequired);
        DomainGuard.IsNullOrEmpty(apiLogin, Errors.PaymentProviderConfigApiLoginIsRequired);
        DomainGuard.IsNullOrEmpty(secretKey, Errors.PaymentProviderConfigSecretKeyIsRequired);

        AccountId = accountId;
        MerchantId = merchantId;
        ApiKey = apiKey;
        ApiLogin = apiLogin;
        SecretKey = secretKey;
        Currency = currency;
        PaymentCountry = paymentCountry;
        IsTest = isTest;
        NotificationUrl = notificationUrl;
        RequiresDisbursement = requiresDisbursement;
        IsActive = isActive;
        UpdatedBy = updatedBy;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public void Delete(Guid deletedBy)
    {
        IsActive = false;
        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = SystemClock.Instance.GetCurrentInstant();
    }
}
