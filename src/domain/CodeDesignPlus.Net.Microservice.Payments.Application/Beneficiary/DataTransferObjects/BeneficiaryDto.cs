using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.DataTransferObjects;

public class BeneficiaryDto : IDtoBase
{
    public required Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BankCode { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public AccountType AccountType { get; set; }
    public string AccountNumber { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public string HolderName { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public string? SwiftBic { get; set; }
    public string? Iban { get; set; }
    public string? RoutingNumber { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public Guid Tenant { get; set; }
}
