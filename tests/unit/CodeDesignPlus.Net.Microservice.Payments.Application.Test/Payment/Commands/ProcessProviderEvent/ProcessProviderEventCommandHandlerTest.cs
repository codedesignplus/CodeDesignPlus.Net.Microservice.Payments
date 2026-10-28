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
        // Arrange
        collect.Setup(x => x.IsAuthentic(Body, It.IsAny<MerchantCredentials>())).Returns(false);

        // Act
        await Handle();

        // Assert
        mediator.Verify(x => x.Send(It.IsAny<UpdateStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
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

    private void Notify(string reference, PaymentStatus status)
    {
        var outcome = new CollectOutcome(status, "tx-1", Money.FromLong(119_000L, "COP"), "CARD", new Dictionary<string, string?> { ["status"] = status.ToString() });

        collect.Setup(x => x.ReadNotificationAsync(Body, It.IsAny<CancellationToken>())).ReturnsAsync(new ProviderNotification(reference, outcome));
    }

    private Task Handle()
    {
        var handler = new ProcessProviderEventCommandHandler(repository.Object, providers.Object, resolver.Object, mediator.Object, Mock.Of<ILogger<ProcessProviderEventCommandHandler>>());

        return handler.Handle(new ProcessProviderEventCommand("wompi", Body), CancellationToken.None);
    }
}
