using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class BeneficiaryAggregate(Guid id) : AggregateRootBase(id)
{
    public Guid UserId { get; private set; }
    public string BankCode { get; private set; } = null!;
    public string BankName { get; private set; } = null!;
    public AccountType AccountType { get; private set; }
    public string AccountNumber { get; private set; } = null!;
    public string DocumentType { get; private set; } = null!;
    public string DocumentNumber { get; private set; } = null!;
    public string HolderName { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public string Currency { get; private set; } = null!;
    public string? SwiftBic { get; private set; }
    public string? Iban { get; private set; }
    public string? RoutingNumber { get; private set; }
    public bool IsVerified { get; private set; }
    public Guid Tenant { get; private set; }

    public static BeneficiaryAggregate Create(
        Guid id,
        Guid userId,
        string bankCode,
        string bankName,
        AccountType accountType,
        string accountNumber,
        string documentType,
        string documentNumber,
        string holderName,
        string country,
        string currency,
        string? swiftBic,
        string? iban,
        string? routingNumber,
        Guid tenant)
    {
        DomainGuard.GuidIsEmpty(id, Errors.PaymentIdCannotBeEmpty);
        DomainGuard.GuidIsEmpty(userId, Errors.BeneficiaryUserIdIsRequired);
        DomainGuard.IsNullOrEmpty(bankCode, Errors.BankCodeIsRequired);
        DomainGuard.IsNullOrEmpty(bankName, Errors.BankNameIsRequired);
        DomainGuard.IsNullOrEmpty(accountNumber, Errors.AccountNumberIsRequired);
        DomainGuard.IsNullOrEmpty(documentType, Errors.DocumentTypeIsRequired);
        DomainGuard.IsNullOrEmpty(documentNumber, Errors.DocumentNumberIsRequired);
        DomainGuard.IsNullOrEmpty(holderName, Errors.HolderNameIsRequired);
        DomainGuard.IsNullOrEmpty(country, Errors.CountryIsRequired);
        DomainGuard.IsNullOrEmpty(currency, Errors.CurrencyIsRequired);

        var aggregate = new BeneficiaryAggregate(id)
        {
            UserId = userId,
            BankCode = bankCode,
            BankName = bankName,
            AccountType = accountType,
            AccountNumber = accountNumber,
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            HolderName = holderName,
            Country = country,
            Currency = currency,
            SwiftBic = swiftBic,
            Iban = iban,
            RoutingNumber = routingNumber,
            IsVerified = false,
            IsActive = true,
            Tenant = tenant,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        return aggregate;
    }

    public void Update(
        string bankCode,
        string bankName,
        AccountType accountType,
        string accountNumber,
        string documentType,
        string documentNumber,
        string holderName,
        string country,
        string currency,
        string? swiftBic,
        string? iban,
        string? routingNumber)
    {
        DomainGuard.IsNullOrEmpty(bankCode, Errors.BankCodeIsRequired);
        DomainGuard.IsNullOrEmpty(bankName, Errors.BankNameIsRequired);
        DomainGuard.IsNullOrEmpty(accountNumber, Errors.AccountNumberIsRequired);
        DomainGuard.IsNullOrEmpty(documentType, Errors.DocumentTypeIsRequired);
        DomainGuard.IsNullOrEmpty(documentNumber, Errors.DocumentNumberIsRequired);
        DomainGuard.IsNullOrEmpty(holderName, Errors.HolderNameIsRequired);
        DomainGuard.IsNullOrEmpty(country, Errors.CountryIsRequired);
        DomainGuard.IsNullOrEmpty(currency, Errors.CurrencyIsRequired);

        BankCode = bankCode;
        BankName = bankName;
        AccountType = accountType;
        AccountNumber = accountNumber;
        DocumentType = documentType;
        DocumentNumber = documentNumber;
        HolderName = holderName;
        Country = country;
        Currency = currency;
        SwiftBic = swiftBic;
        Iban = iban;
        RoutingNumber = routingNumber;
        IsVerified = false; // Reset verification on update

        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public void Verify()
    {
        IsVerified = true;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }
}
