using System.Collections.Generic;
using System.Linq;
using CodeDesignPlus.Net.Core.Abstractions;
using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Domain;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using NodaTime;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Test;

/// <summary>
/// A declined attempt waits for the provider's retry window instead of closing the payment, and an approval that comes after
/// the payment closed is kept as a late approval instead of being rejected (pendings/622).
/// </summary>
public class PaymentRetryWindowTest
{
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    private static readonly Guid Creator = Guid.Parse("cc000000-0000-4000-8000-000000000003");
    private static readonly Instant WindowEnd = SystemClock.Instance.GetCurrentInstant() + Duration.FromMinutes(3);

    [Fact]
    public void DeclineAttempt_InProgress_AwaitsTheRetryUntilTheWindowEnds()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.DeclineAttempt(Declined(), "tx-1", "CARD", PaymentFailureReason.Declined, "Fondos insuficientes", WindowEnd);

        // Assert
        Assert.Equal((PaymentStatus.AwaitingRetry, WindowEnd), (payment.Status, payment.RetryWindowEndsAt));
    }

    [Fact]
    public void DeclineAttempt_InProgress_PublishesTheDeclinedAttemptAndNoOutcome()
    {
        // Arrange
        var payment = Payment();

        // Act
        payment.DeclineAttempt(Declined(), "tx-1", "CARD", PaymentFailureReason.Declined, "Fondos insuficientes", WindowEnd);

        // Assert
        var events = payment.GetAndClearEvents();

        Assert.Equal((1, 0), (events.OfType<PaymentAttemptDeclinedDomainEvent>().Count(), events.OfType<PaymentResponseAssociatedDomainEvent>().Count()));
    }

    [Fact]
    public void DeclineAttempt_AlreadyAwaitingRetry_KeepsTheFirstWindowAndPublishesNothing()
    {
        // Arrange
        var payment = Payment();

        payment.DeclineAttempt(Declined(), "tx-1", "CARD", PaymentFailureReason.Declined, null, WindowEnd);
        payment.GetAndClearEvents();

        // Act
        payment.DeclineAttempt(Declined(), "tx-2", "PSE", PaymentFailureReason.Declined, null, WindowEnd + Duration.FromMinutes(3));

        // Assert
        Assert.Equal((WindowEnd, 0), (payment.RetryWindowEndsAt!.Value, payment.GetAndClearEvents().Count));
    }

    [Fact]
    public void IsAwaitingRetryAt_AfterTheWindow_IsFalse()
    {
        // Arrange
        var payment = Payment();

        payment.DeclineAttempt(Declined(), "tx-1", "CARD", PaymentFailureReason.Declined, null, WindowEnd);

        // Act
        var awaiting = payment.IsAwaitingRetryAt(WindowEnd);

        // Assert
        Assert.False(awaiting);
    }

    [Fact]
    public void SetFinalResponse_RetryApprovedInsideTheWindow_ClosesAsAnOrdinaryApproval()
    {
        // Arrange
        var payment = Payment();

        payment.DeclineAttempt(Declined(), "tx-1", "CARD", PaymentFailureReason.Declined, "Fondos insuficientes", WindowEnd);
        payment.GetAndClearEvents();

        // Act
        payment.SetFinalResponse(PaymentStatus.Succeeded, Approved(), "tx-2", Money.FromLong(119_000L, "COP"), "PSE");

        // Assert
        var response = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();

        Assert.Equal((PaymentStatus.Succeeded, false, PaymentFailureReason.None), (response.Status, response.LateApproval, payment.FailureReason));
    }

    [Fact]
    public void SetFinalResponse_WindowClosedWithoutRetry_ClosesAsFailed()
    {
        // Arrange
        var payment = Payment();

        payment.DeclineAttempt(Declined(), "tx-1", "CARD", PaymentFailureReason.Declined, null, WindowEnd);
        payment.GetAndClearEvents();

        // Act
        payment.SetFinalResponse(PaymentStatus.Failed, Declined(), "tx-1", failureReason: PaymentFailureReason.Declined);

        // Assert
        Assert.Equal(PaymentStatus.Failed, payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single().Status);
    }

    [Theory]
    [InlineData(PaymentStatus.Failed)]
    [InlineData(PaymentStatus.Expired)]
    public void SetFinalResponse_ApprovalAfterTheClose_IsKeptAsALateApproval(PaymentStatus closedAs)
    {
        // Arrange
        var payment = Close(Payment(), closedAs);

        // Act
        payment.SetFinalResponse(PaymentStatus.Succeeded, Approved(), "tx-2", Money.FromLong(119_000L, "COP"), "PSE");

        // Assert
        var response = payment.GetAndClearEvents().OfType<PaymentResponseAssociatedDomainEvent>().Single();

        Assert.Equal((PaymentStatus.Succeeded, true, true, 119_000L), (payment.Status, payment.ApprovedLate, response.LateApproval, response.CapturedAmount!.Amount));
    }

    [Fact]
    public void SetFinalResponse_FailureAfterAnApproval_IsRejected()
    {
        // Arrange
        var payment = Payment();

        payment.SetFinalResponse(PaymentStatus.Succeeded, Approved(), "tx-1", Money.FromLong(119_000L, "COP"), "CARD");

        // Act
        var exception = Record.Exception(() => payment.SetFinalResponse(PaymentStatus.Failed, Declined(), "tx-2"));

        // Assert
        Assert.IsAssignableFrom<CodeDesignPlusException>(exception);
    }

    private static PaymentAggregate Close(PaymentAggregate payment, PaymentStatus closedAs)
    {
        if (closedAs == PaymentStatus.Expired)
        {
            payment.Expire("La página de pago venció.");
        }
        else
        {
            payment.SetFinalResponse(PaymentStatus.Failed, Declined(), "tx-1", failureReason: PaymentFailureReason.Declined);
        }

        payment.GetAndClearEvents();

        return payment;
    }

    private static Dictionary<string, string?> Declined() => new() { ["status"] = "DECLINED" };

    private static Dictionary<string, string?> Approved() => new() { ["status"] = "APPROVED" };

    private static PaymentAggregate Payment()
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");
        var buyer = Buyer.CreateWithoutShipping(Guid.NewGuid(), "Ana Restrepo", "3001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        return PaymentAggregate.Create(Guid.NewGuid(), "CommonAreas", Guid.NewGuid(), subTotal, tax, subTotal + tax, buyer, "Reserva", "Wompi", false, Tenant, Creator);
    }
}
