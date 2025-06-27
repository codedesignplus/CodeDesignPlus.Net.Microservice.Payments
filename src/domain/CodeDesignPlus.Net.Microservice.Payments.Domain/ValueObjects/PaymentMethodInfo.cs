using System.Text.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public sealed partial class PaymentMethod
{

    public string Type { get; private set; }

    public CreditCard? CreditCard { get; private set; }
    public Pse? Pse { get; private set; }

    [JsonConstructor]
    public PaymentMethod(string type, CreditCard? creditCard, Pse? pse)
    {
        ApplicationGuard.IsNullOrEmpty(type, Errors.PaymentMethodCannotBeNullOrEmpty);
        ApplicationGuard.IsTrue(creditCard is null && pse is null, Errors.PaymentMethodInfoMustHaveOnePaymentMethod);

        this.CreditCard = creditCard;
        this.Pse = pse;
        this.Type = type;


    }
    public static PaymentMethod Create(string paymentMethod, CreditCard? creditCard = null, Pse? pse = null)
    {
        return new PaymentMethod(paymentMethod, creditCard, pse);
    }
}