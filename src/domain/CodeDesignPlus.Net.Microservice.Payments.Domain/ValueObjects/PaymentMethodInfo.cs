using System.Text.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public sealed partial class PaymentMethodInfo
{

    public string Type { get; private set; }
    public string Brand { get; private set; } = null!;
    public string MaskedIdentifier { get; private set; } = null!;

    [JsonConstructor]
    public PaymentMethodInfo(string type, string brand, string maskedIdentifier)
    {
        DomainGuard.IsNullOrEmpty(type, Errors.PaymentMethodTypeCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(type.Length, 50, Errors.PaymentMethodTypeCannotBeGreaterThan50Characters);

        DomainGuard.IsNullOrEmpty(brand, Errors.PaymentMethodBrandCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(brand.Length, 50, Errors.PaymentMethodBrandCannotBeGreaterThan50Characters);

        DomainGuard.IsNullOrEmpty(maskedIdentifier, Errors.PaymentMethodMaskedIdentifierCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(maskedIdentifier.Length, 50, Errors.PaymentMethodMaskedIdentifierCannotBeGreaterThan50Characters);

        this.Type = type;
        this.Brand = brand;
        this.MaskedIdentifier = maskedIdentifier;
    }

    public static PaymentMethodInfo Create(string type, string brand, string maskedIdentifier)
    {
        return new PaymentMethodInfo(type, brand, maskedIdentifier);
    }
}