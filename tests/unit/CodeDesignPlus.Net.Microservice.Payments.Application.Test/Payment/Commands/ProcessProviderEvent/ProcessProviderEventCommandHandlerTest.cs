using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ProcessProviderEvent;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using MediatR;
using Microsoft.Extensions.Logging;
using Xunit;
using AppErrors = CodeDesignPlus.Net.Microservice.Payments.Application.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Commands.ProcessProviderEvent;

/// <summary>
/// The single webhook trusts an event only after checking it with the secret of the account its reference points to.
/// </summary>
public class ProcessProviderEventCommandHandlerTest
{
    private const string Body = "{\"event\":\"transaction.updated\"}";

    private readonly PaymentAggregate payment = TestData.Payment();
    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<ICollectProvider> collect = new();
    private readonly Mock<IPaymentProviderFactory> providers = new();
    private readonly Mock<IMerchantCredentialsResolver> resolver = new();
    private readonly Mock<IMediator> mediator = new();
    private readonly Mock<IMerchantAccountHealth> health = new();
    private readonly Mock<IPubSub> pubsub = new();

    public ProcessProviderEventCommandHandlerTest()
    {
        collect.SetupGet(x => x.Code).Returns("Wompi");
        providers.Setup(x => x.GetCollect("wompi")).Returns(collect.Object);
        repository.Setup(x => x.FindAsync<PaymentAggregate>(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        resolver.Setup(x => x.ForPaymentAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Credentials());
        collect.Setup(x => x.IsAuthentic(Body, It.IsAny<MerchantCredentials>())).Returns(true);
        Notify(payment.Id.ToString(), PaymentStatus.Succeeded);
    }

    [Fact]
    public async Task Handle_AuthenticEvent_ClosesThePaymentWithWhatTheProviderCaptured()
    {
        // Act
        await Handle();

        // Assert
        mediator.Verify(x => x.Send(It.Is<UpdateStatusCommand>(c => c.Id == payment.Id && c.Status == PaymentStatus.Succeeded && c.CapturedAmount!.Amount == 119_000L && c.ProviderTransactionId == "tx-1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SignatureOfAnotherSecret_IsDiscarded()
    {
        // Arrange: a forged event; the provider knows nothing of what it claims.
        collect.Setup(x => x.IsAuthentic(Body, It.IsAny<MerchantCredentials>())).Returns(false);

        // Act
        await Handle();

        // Assert
        mediator.Verify(x => x.Send(It.IsAny<UpdateStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ForgedEvent_DoesNotCountAgainstTheAccount()
    {
        // Arrange: whoever knows a payment id must not be able to invalidate its account by posting bad signatures.
        collect.Setup(x => x.IsAuthentic(Body, It.IsAny<MerchantCredentials>())).Returns(false);
        collect.Setup(x => x.GetOutcomeAsync(payment, It.IsAny<MerchantCredentials>(), It.IsAny<CancellationToken>())).ReturnsAsync(Outcome("tx-other", PaymentStatus.Failed));

        // Act
        await Handle();

        // Assert
        health.Verify(x => x.ReportRejectionAsync(It.IsAny<PaymentAggregate>(), It.IsAny<Error>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GenuineEventWithWrongStoredSecret_AppliesTheProviderAnswerAndCountsARejection()
    {
        // Arrange: the provider confirms the same transaction, so the event was genuine and the stored secret is wrong.
        collect.Setup(x => x.IsAuthentic(Body, It.IsAny<MerchantCredentials>())).Returns(false);
        collect.Setup(x => x.GetOutcomeAsync(payment, It.IsAny<MerchantCredentials>(), It.IsAny<CancellationToken>())).ReturnsAsync(Outcome("tx-1", PaymentStatus.Succeeded));

        // Act
        await Handle();

        // Assert
        health.Verify(x => x.ReportRejectionAsync(payment, AppErrors.MerchantEventsSecretDoesNotVerify, It.IsAny<CancellationToken>()), Times.Once);
        mediator.Verify(x => x.Send(It.Is<UpdateStatusCommand>(c => c.Status == PaymentStatus.Succeeded), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AuthenticEvent_ReportsTheKeysWork()
    {
        // Act
        await Handle();

        // Assert
        health.Verify(x => x.ReportAcceptanceAsync(payment, It.IsAny<MerchantCredentials>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownReference_IsDiscardedWithoutReadingAnySecret()
    {
        // Arrange
        Notify(Guid.NewGuid().ToString(), PaymentStatus.Succeeded);

        // Act
        await Handle();

        // Assert
        resolver.Verify(x => x.ForPaymentAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
        mediator.Verify(x => x.Send(It.IsAny<UpdateStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReferenceThatIsNotAPaymentId_IsDiscarded()
    {
        // Arrange
        Notify("stage0-card_approved-ee308018", PaymentStatus.Succeeded);

        // Act
        await Handle();

        // Assert
        mediator.Verify(x => x.Send(It.IsAny<UpdateStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AccountWithoutKeys_IsDiscarded()
    {
        // Arrange
        resolver.Setup(x => x.ForPaymentAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync((MerchantCredentials?)null);

        // Act
        await Handle();

        // Assert
        mediator.Verify(x => x.Send(It.IsAny<UpdateStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PendingTransaction_DoesNotCloseThePayment()
    {
        // Arrange
        Notify(payment.Id.ToString(), PaymentStatus.Pending);

        // Act
        await Handle();

        // Assert
        mediator.Verify(x => x.Send(It.IsAny<UpdateStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AuthenticRefundOfAKnownTransaction_RecordsItAndPublishesIt()
    {
        // Arrange: the refund's reference is its own, so the payment is found by the transaction.
        RefundEvent();

        // Act
        await Handle();

        // Assert
        Assert.Equal(RefundStatus.Approved, payment.Refunds!.Single().Status);
        pubsub.Verify(x => x.PublishAsync(It.Is<IReadOnlyList<IDomainEvent>>(e => e.OfType<Domain.DomainEvents.PaymentRefundUpdatedDomainEvent>().Any(r => r.RefundId == "COa7Bk9m1" && r.Amount!.Amount == 50_000L)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RefundWithAnotherSignature_IsDiscarded()
    {
        // Arrange
        RefundEvent();
        collect.Setup(x => x.IsAuthentic(Body, It.IsAny<MerchantCredentials>())).Returns(false);

        // Act
        await Handle();

        // Assert
        Assert.Null(payment.Refunds);
        pubsub.Verify(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void RefundEvent()
    {
        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "1292-1602113476-10985", Money.FromLong(119_000L, "COP"), "CARD");
        payment.GetAndClearEvents();

        var refunds = new Mock<IRefundProvider>();

        refunds.SetupGet(x => x.Code).Returns("Wompi");
        refunds.Setup(x => x.ReadRefundNotificationAsync(Body, It.IsAny<CancellationToken>())).ReturnsAsync(new ProviderRefundNotification("ORDER-2024-001", "1292-1602113476-10985", "COa7Bk9m1", RefundStatus.Approved, Money.FromLong(50_000L, "COP"), null, null));
        collect.Setup(x => x.ReadNotificationAsync(Body, It.IsAny<CancellationToken>())).ReturnsAsync((ProviderNotification?)null);
        providers.Setup(x => x.FindRefunds("Wompi")).Returns(refunds.Object);
        repository.Setup(x => x.FindByProviderTransactionAsync("Wompi", "1292-1602113476-10985", It.IsAny<CancellationToken>())).ReturnsAsync(payment);
    }

    private static CollectOutcome Outcome(string transactionId, PaymentStatus status) => new(status, transactionId, Money.FromLong(119_000L, "COP"), "CARD", new Dictionary<string, string?> { ["status"] = status.ToString() });

    private void Notify(string reference, PaymentStatus status)
    {
        var outcome = new CollectOutcome(status, "tx-1", Money.FromLong(119_000L, "COP"), "CARD", new Dictionary<string, string?> { ["status"] = status.ToString() });

        collect.Setup(x => x.ReadNotificationAsync(Body, It.IsAny<CancellationToken>())).ReturnsAsync(new ProviderNotification(reference, outcome));
    }

    private Task Handle()
    {
        var handler = new ProcessProviderEventCommandHandler(repository.Object, providers.Object, resolver.Object, health.Object, mediator.Object, pubsub.Object, Mock.Of<ILogger<ProcessProviderEventCommandHandler>>());

        return handler.Handle(new ProcessProviderEventCommand("wompi", Body), CancellationToken.None);
    }
}
