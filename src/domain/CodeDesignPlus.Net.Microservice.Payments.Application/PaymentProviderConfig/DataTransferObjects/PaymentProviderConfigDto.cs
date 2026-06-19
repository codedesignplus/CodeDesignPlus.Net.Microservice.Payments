namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.DataTransferObjects;

public class PaymentProviderConfigDto : IDtoBase
{
    public required Guid Id { get; set; }
    public PaymentProvider Provider { get; set; }
    public int AccountId { get; set; }
    public string MerchantId { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiLogin { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public string PaymentCountry { get; set; } = null!;
    public bool IsTest { get; set; }
    public bool IsActive { get; set; }
    public Guid Tenant { get; set; }
}
