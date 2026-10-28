using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

public class PaymentDto : IDtoBase
{
    public required Guid Id { get; set; }
    public string Module { get; set; } = null!;
    public Guid? Tenant { get; set; }
    public PaymentStatus Status { get; set; }
    public Net.ValueObjects.Financial.Money SubTotal { get; set; } = null!;
    public Net.ValueObjects.Financial.Money Tax { get; set; } = null!;
    public Net.ValueObjects.Financial.Money Total { get; set; } = null!;
    // Sin Buyer a proposito: el mapeo nunca lo copio (siempre salia null) y nadie lo lee, y copiarlo publicaria el
    // documento del comprador, que MapsterConfig evita exponer (pendings/174).
    public string Description { get; set; } = null!;

    public string Provider { get; set; } = null!;
    public bool PlatformAccount { get; set; }
    public string? ProviderTransactionId { get; set; }
    public Net.ValueObjects.Financial.Money? CapturedAmount { get; set; }
    public string? PaymentMethodType { get; set; }
    public Dictionary<string, string?> InitiateResponse { get; set; } = [];
    public Dictionary<string, string?> FinalResponse { get; set; } = [];
}