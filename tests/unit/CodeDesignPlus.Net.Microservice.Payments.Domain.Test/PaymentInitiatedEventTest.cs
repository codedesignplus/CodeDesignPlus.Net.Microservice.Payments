using System.Linq;
using CodeDesignPlus.Net.Microservice.Payments.Domain;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using Xunit;
using Card = CodeDesignPlus.Net.ValueObjects.Payment.CreditCard;
using Method = CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Test;

/// <summary>
/// El evento de cobro iniciado viaja por el bus: no puede llevar el código de seguridad de la tarjeta (pendings/322).
/// </summary>
public class PaymentInitiatedEventTest
{
    private const string SecurityCodeProbe = "cvv-probe-322";

    [Fact]
    public void Create_CardPayment_EventDoesNotCarrySecurityCode()
    {
        // Arrange
        var payment = BuildCardPayment();

        // Act
        var json = JsonSerializer.Serialize(payment.GetAndClearEvents().OfType<PaymentInitiatedDomainEvent>().Single());

        // Assert
        Assert.DoesNotContain(SecurityCodeProbe, json, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_CardPayment_EventCarriesTypeAndLastDigits()
    {
        // Arrange
        var payment = BuildCardPayment();

        // Act
        var @event = payment.GetAndClearEvents().OfType<PaymentInitiatedDomainEvent>().Single();

        // Assert
        Assert.Equal(("VISA", "4242"), (@event.PaymentMethodType, @event.Last4Digits));
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
