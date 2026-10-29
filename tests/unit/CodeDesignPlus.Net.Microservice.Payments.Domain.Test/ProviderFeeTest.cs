using CodeDesignPlus.Net.Microservice.Payments.Domain;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Test;

/// <summary>
/// The commission the provider keeps, with the real payment of 29-oct-2026: $10.000 by PSE to ExcellenceForge's merchant,
/// of which Wompi held back $1.148,35 (on_settlement = $8.851,65).
/// </summary>
public class ProviderFeeTest
{
    private static readonly Money RealGross = Money.FromLong(1_000_000L, "COP");

    private static ProviderFeeRate WompiColombia() => ProviderFeeRate.Create(265, Money.FromLong(70_000L, "COP"), 1900);

    [Fact]
    public void Apply_RealPaymentOf29October_GivesTheCommissionWompiHeldBack()
    {
        // Act
        var fee = WompiColombia().Apply(RealGross)!;

        // Assert
        Assert.Equal((96_500L, 18_335L, 885_165L, ProviderFeeSources.Calculated), (fee.Fee.Amount, fee.Tax.Amount, fee.NetOf(RealGross).Amount, fee.Source));
    }

    [Fact]
    public void Apply_HalfAMinorUnit_RoundsAwayFromZero()
    {
        // Arrange: 2,65 % of 1.001.000 is 26.526,5, which rounds up to 26.527; the 19 % of 96.527 is 18.340,13.
        var gross = Money.FromLong(1_001_000L, "COP");

        // Act
        var fee = WompiColombia().Apply(gross)!;

        // Assert
        Assert.Equal((96_527L, 18_340L), (fee.Fee.Amount, fee.Tax.Amount));
    }

    [Fact]
    public void Apply_FixedAmountInAnotherCurrency_IsUnknown()
    {
        // Act
        var fee = WompiColombia().Apply(Money.FromLong(1_000_000L, "USD"));

        // Assert
        Assert.Null(fee);
    }

    [Fact]
    public void RateFor_MethodWithItsOwnRate_UsesIt()
    {
        // Arrange
        var card = ProviderFeeRate.Create(299, Money.FromLong(90_000L, "COP"), 1900);
        var schedule = ProviderFeeSchedule.Create(WompiColombia(), [new PaymentMethodFeeRate("card", card)]);

        // Act
        var rates = (schedule.RateFor("CARD"), schedule.RateFor("PSE"), schedule.RateFor(null));

        // Assert
        Assert.Equal((card, WompiColombia(), WompiColombia()), rates);
    }

    [Fact]
    public void Create_NegativeRate_IsRejected()
    {
        // Act
        var exception = Record.Exception(() => ProviderFeeRate.Create(-1, Money.FromLong(70_000L, "COP"), 1900));

        // Assert
        Assert.NotNull(exception);
    }

    [Fact]
    public void SetFinalResponse_SucceededWithFee_PublishesTheCommissionAndTheExpectedNet()
    {
        // Arrange
        var payment = Payment(RealGross);

        // Act
        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "1584063-1793277348-41424", RealGross, "PSE", WompiColombia().Apply(RealGross));

        // Assert
        var published = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();

        Assert.Equal((96_500L, 18_335L, 885_165L, "Calculated", false), (published.ProviderFee!.Amount, published.ProviderFeeTax!.Amount, published.ExpectedNetAmount!.Amount, published.FeeSource, published.PlatformAccount));
    }

    [Fact]
    public void SetFinalResponse_Failed_KeepsNoCommission()
    {
        // Arrange
        var payment = Payment(RealGross);

        // Act
        payment.SetFinalResponse(PaymentStatus.Failed, new Dictionary<string, string?> { ["status"] = "DECLINED" }, "tx-1", RealGross, "PSE", WompiColombia().Apply(RealGross));

        // Assert
        var published = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();

        Assert.Equal(((Money?)null, (Money?)null, (string?)null), (published.ProviderFee, published.ExpectedNetAmount, payment.FeeSource));
    }

    [Fact]
    public void RegisterRefund_SameStatusAgain_ChangesNothing()
    {
        // Arrange
        var payment = Succeeded();

        payment.RegisterRefund("COa7Bk9m1", RefundStatus.Pending, Money.FromLong(50_000L, "COP"), null, null);
        payment.GetAndClearEvents();

        // Act
        var changed = payment.RegisterRefund("COa7Bk9m1", RefundStatus.Pending, Money.FromLong(50_000L, "COP"), null, null);

        // Assert
        Assert.Equal((false, 0), (changed, payment.GetAndClearEvents().Count));
    }

    [Fact]
    public void RegisterRefund_LatePendingAfterApproved_KeepsItApproved()
    {
        // Arrange
        var payment = Succeeded();

        payment.RegisterRefund("COa7Bk9m1", RefundStatus.Approved, Money.FromLong(50_000L, "COP"), null, null);

        // Act
        payment.RegisterRefund("COa7Bk9m1", RefundStatus.Pending, null, null, null);

        // Assert
        Assert.Equal(RefundStatus.Approved, payment.Refunds!.Single().Status);
    }

    [Fact]
    public void RegisterRefund_PendingThenDeclined_PublishesEachChange()
    {
        // Arrange
        var payment = Succeeded();

        // Act
        payment.RegisterRefund("COa7Bk9m1", RefundStatus.Pending, Money.FromLong(50_000L, "COP"), null, null);
        payment.RegisterRefund("COa7Bk9m1", RefundStatus.Declined, null, "REFUND_DECLINED", "El procesador rechazó el reembolso");

        // Assert
        var published = payment.GetAndClearEvents().OfType<PaymentRefundUpdatedDomainEvent>().Select(x => (x.Status, x.Amount!.Amount)).ToList();

        Assert.Equal([(RefundStatus.Pending, 50_000L), (RefundStatus.Declined, 50_000L)], published);
    }

    private static PaymentAggregate Succeeded()
    {
        var payment = Payment(RealGross);

        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "tx-1", RealGross, "PSE");
        payment.GetAndClearEvents();

        return payment;
    }

    private static PaymentAggregate Payment(Money total)
    {
        var buyer = Buyer.CreateWithoutShipping(Guid.NewGuid(), "Ana Restrepo", "3001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        return PaymentAggregate.Create(Guid.NewGuid(), "Invoicing", Guid.NewGuid(), total, Money.FromLong(0L, total.Currency), total, buyer, "Cuota", "Wompi", false, Guid.NewGuid(), Guid.NewGuid());
    }
}
