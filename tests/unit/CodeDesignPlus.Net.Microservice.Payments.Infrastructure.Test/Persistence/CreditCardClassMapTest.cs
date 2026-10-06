using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Mongo.Extensions;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Persistence;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Card = CodeDesignPlus.Net.ValueObjects.Payment.CreditCard;
using Method = CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Persistence;

/// <summary>
/// Vigila que el código de seguridad de la tarjeta nunca llegue a Mongo (pendings/322).
/// </summary>
/// <remarks>
/// PCI DSS (requisito 3.2) prohíbe guardar el CVV después de la autorización, incluso cifrado. El valor
/// sonda es inconfundible: si aparece en el documento BSON, la fuga está ahí.
/// </remarks>
public class CreditCardClassMapTest
{
    private const string SecurityCodeProbe = "cvv-probe-322";

    public CreditCardClassMapTest()
    {
        MongoSerializerRegistration.RegisterSerializers();
        MongoClassMaps.Register();
    }

    [Fact]
    public void ToBsonDocument_CardPayment_DoesNotStoreSecurityCode()
    {
        // Arrange
        var payment = BuildCardPayment();

        // Act
        var document = payment.ToBsonDocument();

        // Assert
        Assert.DoesNotContain(SecurityCodeProbe, document.ToJson(), StringComparison.Ordinal);
    }

    [Fact]
    public void ToBsonDocument_CardPayment_KeepsTheDisplayData()
    {
        // Arrange
        var payment = BuildCardPayment();

        // Act
        var card = payment.ToBsonDocument()["PaymentMethod"]["CreditCard"].AsBsonDocument;

        // Assert
        Assert.Equal("4242", card["Last4Digits"].AsString);
    }

    [Fact]
    public void Deserialize_StoredCardWithoutSecurityCode_ReadsThePayment()
    {
        // Arrange: lo que queda en la base después de la purga, o lo que se guarda a partir de ahora.
        var document = BuildCardPayment().ToBsonDocument();
        document["PaymentMethod"]["CreditCard"].AsBsonDocument.Remove("SecurityCode");

        // Act
        var payment = BsonSerializer.Deserialize<PaymentAggregate>(document);

        // Assert
        Assert.Equal("4242", payment.PaymentMethod.CreditCard!.Last4Digits);
    }

    private static PaymentAggregate BuildCardPayment()
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");
        var buyerId = Guid.Parse("cc000000-0000-4000-8000-000000000003");

        var buyer = Buyer.CreateWithoutShipping(buyerId, "Ana Restrepo", "+573001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        var method = Method.Create("VISA", null, Card.Create("tok-probe", "4242", "2030/12", "ANA RESTREPO", SecurityCodeProbe));

        return PaymentAggregate.Create(Guid.Parse("aa000000-0000-4000-8000-000000000001"), "CommonAreas", Guid.Parse("dd000000-0000-4000-8000-000000000004"), subTotal, tax, subTotal + tax, buyer, null, method, "Booking", PaymentProvider.Payu, Guid.Parse("bb000000-0000-4000-8000-000000000002"), buyerId);
    }
}
