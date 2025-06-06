using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public sealed partial class Order
{
    public string Description { get; private set; }
    public Buyer Buyer { get; private set; }
    public Amount Ammount { get; private set; } = null!;
    public Amount Tax { get; private set; } = null!;
    public Amount TaxReturnBase { get; private set; } = null!;

    [JsonConstructor]
    private Order(string description, Buyer buyer, Amount ammount, Amount tax, Amount taxReturnBase)
    {
        DomainGuard.IsNullOrEmpty(description, Errors.DescriptionCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(description.Length, 255, Errors.DescriptionCannotBeGreaterThan255Characters);
        DomainGuard.IsNull(buyer, Errors.BuyerCannotBeNull);
        DomainGuard.IsNull(ammount, Errors.AmountCannotBeNull);
        DomainGuard.IsNull(tax, Errors.TaxCannotBeNull);
        DomainGuard.IsNull(taxReturnBase, Errors.TaxReturnBaseCannotBeNull);

        Description = description;
        Buyer = buyer;
        Ammount = ammount;
        Tax = tax;
        TaxReturnBase = taxReturnBase;
    }
    public static Order Create(string description, Buyer buyer, Amount ammount, Amount tax, Amount taxReturnBase)
    {
        return new Order(description, buyer, ammount, tax, taxReturnBase);
    }
}
