using System.Linq;
using CodeDesignPlus.Net.Microservice.Payments.Domain;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Test;

/// <summary>
/// The payment events travel on the bus and other services consume them.
/// </summary>
public class PaymentInitiatedEventTest
{
    [Fact]
    public void Create_Payment_EventCarriesTheProviderCode()
    {
        // Arrange
        var payment = Build();

        // Act
        var @event = payment.GetAndClearEvents().OfType<PaymentInitiatedDomainEvent>().Single();

        // Assert
        Assert.Equal("Wompi", @event.Provider);
    }

    [Fact]
    public void SetFinalResponse_Succeeded_EventCarriesCapturedAmountAndProvider()
    {
        // Arrange
        var payment = Build();
        payment.GetAndClearEvents();
        var captured = Money.FromLong(119_000L, "COP");

        // Act
        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "tx-1", captured, "CARD");

        // Assert
        var @event = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();
        Assert.Equal((PaymentStatus.Succeeded, 119_000L, "Wompi"), (@event.Status, @event.CapturedAmount!.Amount, @event.Provider));
    }

    [Fact]
    public void SetFinalResponse_Failed_DoesNotKeepACapturedAmount()
    {
        // Arrange: a declined transaction still reports the amount it tried to charge.
        var payment = Build();

        // Act
        payment.SetFinalResponse(PaymentStatus.Failed, new Dictionary<string, string?> { ["status"] = "DECLINED" }, "tx-1", Money.FromLong(119_000L, "COP"), "CARD");

        // Assert
        Assert.Null(payment.CapturedAmount);
    }

    [Fact]
    public void SetFinalResponse_CaptureInAnotherCurrency_Throws()
    {
        // Arrange
        var payment = Build();

        // Act
        var exception = Record.Exception(() => payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "tx-1", Money.FromLong(119_000L, "USD"), "CARD"));

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public void Serialize_SucceededEvent_KeepsTheFieldsTheConsumersRead()
    {
        // Arrange: four consumers read Module, ReferenceId, Status as a number and Tenant. The new fields only add.
        var payment = Build();
        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "tx-1", payment.Total, "CARD");
        var @event = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();

        // Act
        var json = Newtonsoft.Json.Linq.JObject.Parse(JsonSerializer.Serialize(@event));

        // Assert
        Assert.Equal(("CommonAreas", "dd000000-0000-4000-8000-000000000004", 2L, "bb000000-0000-4000-8000-000000000002"), (json.Value<string>("Module"), json.Value<string>("ReferenceId"), json.Value<long>("Status"), json.Value<string>("Tenant")));
    }

    private static PaymentAggregate Build()
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");
        var buyerId = Guid.Parse("cc000000-0000-4000-8000-000000000003");

        var buyer = Buyer.CreateWithoutShipping(buyerId, "Ana Restrepo", "+573001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        return PaymentAggregate.Create(Guid.Parse("aa000000-0000-4000-8000-000000000001"), "CommonAreas", Guid.Parse("dd000000-0000-4000-8000-000000000004"), subTotal, tax, subTotal + tax, buyer, "Booking", "Wompi", false, Guid.Parse("bb000000-0000-4000-8000-000000000002"), buyerId);
    }
}
