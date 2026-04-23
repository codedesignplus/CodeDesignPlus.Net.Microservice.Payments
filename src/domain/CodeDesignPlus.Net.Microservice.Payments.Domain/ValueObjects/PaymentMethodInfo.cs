using System.Text.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public sealed partial record PaymentMethod
{

    public string Type { get; private set; }

    public CreditCardToken? CreditCardToken { get; private set; }
    public Pse? Pse { get; private set; }

    [JsonConstructor]
    public PaymentMethod(string type, CreditCardToken? creditCardToken, Pse? pse)
    {
        DomainGuard.IsNullOrEmpty(type, Errors.PaymentMethodCannotBeNullOrEmpty);

        var bothAreNull = creditCardToken == null && pse == null;
        var bothAreProvided = creditCardToken != null && pse != null;

        DomainGuard.IsTrue(bothAreNull || bothAreProvided, Errors.PaymentMethodMustHaveExactlyOneOption);

        this.CreditCardToken = creditCardToken;
        this.Pse = pse;
        this.Type = type;


    }
    public static PaymentMethod Create(string paymentMethod, CreditCardToken? creditCardToken = null, Pse? pse = null)
    {
        return new PaymentMethod(paymentMethod, creditCardToken, pse);
    }
}