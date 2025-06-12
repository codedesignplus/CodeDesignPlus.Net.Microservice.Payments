using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentMethod.DataTransferObjects;

public class PaymentMethodDto : IDtoBase
{
    public required Guid Id { get; set; }
    public Provider Provider { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public TypePaymentMethod Type { get; set; }
    public string? Comments { get; set; } = null!;
}