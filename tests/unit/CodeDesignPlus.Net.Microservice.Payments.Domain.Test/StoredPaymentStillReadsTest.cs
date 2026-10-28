using CodeDesignPlus.Net.Microservice.Payments.Domain;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Mongo.Extensions;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Test;

/// <summary>
/// A payment stored before the provider became a code keeps reading (rule 31): the members added by pendings/660 are
/// loose members, not constructor arguments, so a document without them reads them as absent.
/// </summary>
public class StoredPaymentStillReadsTest
{
    private static readonly string[] AddedBy660 = ["Provider", "PlatformAccount", "ProviderTransactionId", "CapturedAmount", "PaymentMethodType"];

    public StoredPaymentStillReadsTest()
    {
        MongoSerializerRegistration.RegisterSerializers();
    }

    [Fact]
    public void Deserialize_DocumentWithoutTheNewMembers_ReadsThemAsAbsent()
    {
        // Arrange
        var document = Payment().ToBsonDocument();

        foreach (var member in AddedBy660)
        {
            document.Remove(member);
        }

        // Act
        var payment = BsonSerializer.Deserialize<PaymentAggregate>(document);

        // Assert
        Assert.Equal(((string?)null, false, (Money?)null, (string?)null), (payment.Provider, payment.PlatformAccount, payment.CapturedAmount, payment.PaymentMethodType));
    }

    private static PaymentAggregate Payment()
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");
        var buyer = Buyer.CreateWithoutShipping(Guid.NewGuid(), "Ana Restrepo", "3001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        var payment = PaymentAggregate.Create(Guid.NewGuid(), "Invoicing", Guid.NewGuid(), subTotal, tax, subTotal + tax, buyer, "Cuota", "Wompi", false, Guid.NewGuid(), Guid.NewGuid());

        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "tx-1", subTotal + tax, "CARD");

        return payment;
    }
}
