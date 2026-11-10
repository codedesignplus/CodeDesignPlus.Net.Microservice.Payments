using System.Collections.Generic;
using System.Linq;
using CodeDesignPlus.Net.Core.Abstractions;
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
/// A closed payment keeps why it did not succeed and what the bank said, and its event carries both (pendings/622).
/// </summary>
public class PaymentFailureReasonTest
{
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    private static readonly Guid Creator = Guid.Parse("cc000000-0000-4000-8000-000000000003");

    public PaymentFailureReasonTest()
    {
        MongoSerializerRegistration.RegisterSerializers();
    }

    [Fact]
    public void SetFinalResponse_Declined_KeepsTheReasonAndTheBankMessage()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.SetFinalResponse(PaymentStatus.Failed, Declined(), "tx-1", failureReason: PaymentFailureReason.Declined, providerMessage: "  Fondos insuficientes ");

        // Assert
        Assert.Equal((PaymentFailureReason.Declined, "Fondos insuficientes"), (payment.FailureReason, payment.ProviderMessage));
    }

    [Fact]
    public void SetFinalResponse_Declined_PublishesTheReasonAndTheBankMessage()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.SetFinalResponse(PaymentStatus.Failed, Declined(), "tx-1", failureReason: PaymentFailureReason.Declined, providerMessage: "Fondos insuficientes");

        // Assert
        var response = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();

        Assert.Equal((PaymentFailureReason.Declined, "Fondos insuficientes"), (response.FailureReason, response.ProviderMessage));
    }

    [Fact]
    public void SetFinalResponse_LongMessage_IsCutToTheStandardLength()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.SetFinalResponse(PaymentStatus.Failed, Declined(), "tx-1", failureReason: PaymentFailureReason.ProviderError, providerMessage: new string('x', FieldLength.Description + 40));

        // Assert
        Assert.Equal(FieldLength.Description, payment.ProviderMessage!.Length);
    }

    [Fact]
    public void SetFinalResponse_Succeeded_KeepsNoReasonNorMessage()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "tx-1", payment.Total, "CARD", failureReason: PaymentFailureReason.Declined, providerMessage: "Aprobada");

        // Assert
        Assert.Equal((PaymentFailureReason.None, (string?)null), (payment.FailureReason, payment.ProviderMessage));
    }

    [Fact]
    public void SetFinalResponse_ExpiredByTheProvider_IsExpired()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.SetFinalResponse(PaymentStatus.Expired, new Dictionary<string, string?> { ["status"] = "EXPIRED" });

        // Assert
        Assert.Equal(PaymentFailureReason.Expired, payment.FailureReason);
    }

    [Fact]
    public void Expire_AbandonedPayment_PublishesItAsExpired()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.Expire("La página de pago venció sin que se iniciara ninguna transacción.");

        // Assert
        var response = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();

        Assert.Equal((PaymentFailureReason.Expired, PaymentFailureReason.Expired), (payment.FailureReason, response.FailureReason));
    }

    [Fact]
    public void Deserialize_PaymentClosedBeforeTheReason_ReadsItAsNone()
    {
        // Arrange: the payments closed before pendings/622 have neither member (rule 31).
        var payment = Payment();

        payment.SetFinalResponse(PaymentStatus.Failed, Declined(), "tx-1", failureReason: PaymentFailureReason.Declined, providerMessage: "Fondos insuficientes");

        var document = payment.ToBsonDocument();

        document.Remove("FailureReason");
        document.Remove("ProviderMessage");

        // Act
        var read = BsonSerializer.Deserialize<PaymentAggregate>(document);

        // Assert
        Assert.Equal((PaymentFailureReason.None, (string?)null), (read.FailureReason, read.ProviderMessage));
    }

    [Theory]
    [InlineData("bb000000-0000-4000-8000-000000000002", "00000000-0000-0000-0000-000000000000", true)]
    [InlineData("dd000000-0000-4000-8000-000000000004", "cc000000-0000-4000-8000-000000000003", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", "cc000000-0000-4000-8000-000000000003", true)]
    [InlineData("dd000000-0000-4000-8000-000000000004", "ee000000-0000-4000-8000-000000000005", false)]
    [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000", false)]
    public void IsVisibleTo_TenantOrCreator_DecidesWhoReadsIt(string tenant, string user, bool expected)
    {
        // Arrange
        var payment = Payment();

        // Act
        var visible = payment.IsVisibleTo(Guid.Parse(tenant), Guid.Parse(user));

        // Assert
        Assert.Equal(expected, visible);
    }

    [Fact]
    public void IsVisibleTo_PlatformPaymentWithoutTenant_IsNotVisibleToAnotherUser()
    {
        // Arrange: a license is charged to the platform account and may carry no tenant.
        var payment = Payment(platformAccount: true);

        // Act
        var visible = payment.IsVisibleTo(Tenant, Guid.NewGuid());

        // Assert
        Assert.False(visible);
    }

    /// <summary>
    /// The approval says with what the payer paid: the module that invoices it turns it into the tax authority's payment
    /// means code (Kappali pendings/661).
    /// </summary>
    [Fact]
    public void SetFinalResponse_ApprovedCreditCard_PublishesTheInstrumentAndTheCardType()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "tx-1", Money.FromLong(119_000L, "COP"), "CARD", cardType: " CREDIT ");

        // Assert
        var response = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();

        Assert.Equal(("CARD", "CREDIT", "CREDIT"), (response.PaymentMethodType, response.CardType, payment.CardType));
    }

    /// <summary>
    /// When the provider does not say whether the card was credit or debit, nobody guesses it.
    /// </summary>
    [Fact]
    public void SetFinalResponse_CardWithoutType_PublishesNoCardType()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "tx-1", Money.FromLong(119_000L, "COP"), "CARD", cardType: "  ");

        // Assert
        var response = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();

        Assert.Equal(("CARD", (string?)null), (response.PaymentMethodType, response.CardType));
    }

    private static Dictionary<string, string?> Declined() => new() { ["status"] = "DECLINED", ["statusMessage"] = "Fondos insuficientes" };

    private static PaymentAggregate Payment(bool platformAccount = false)
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");
        var buyer = Buyer.CreateWithoutShipping(Guid.NewGuid(), "Ana Restrepo", "3001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");
        var module = platformAccount ? PaymentAggregate.LicensesModule : "Invoicing";
        Guid? tenant = platformAccount ? null : Tenant;

        return PaymentAggregate.Create(Guid.NewGuid(), module, Guid.NewGuid(), subTotal, tax, subTotal + tax, buyer, "Cuota", "Wompi", platformAccount, tenant, Creator);
    }
}
