using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Fees;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ExpireStalePayments;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.RefreshPaymentStatus;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Commands;

/// <summary>
/// The provider's approval and the platform's closing of a payment can arrive at the same time (pendings/665, rule 59).
/// </summary>
/// <remarks>
/// The reconciliation job closes every minute a payment whose page lapsed with no transaction, and the daily sweep closes
/// the ones nobody answered. If the webhook approves the payment between the read and the write of either, the approval
/// wins: before, the closing overwrote it and the module heard «approved» and then «expired» for the same payment. The
/// store copies on read and checks the version on write, as Mongo and the SDK do; a mock that shares one instance would
/// hide the race.
/// </remarks>
public class PaymentRaceTest
{
    private static readonly TimeSpan Grace = TimeSpan.FromMinutes(5);

    private readonly VersionedPaymentStore store = new();
    private readonly Mock<IPubSub> pubsub = new();
    private readonly Mock<ICollectProvider> collect = new();
    private readonly Mock<IPaymentProviderFactory> providers = new();
    private readonly Mock<IMerchantCredentialsResolver> resolver = new();
    private readonly List<IDomainEvent> published = [];
    private readonly PaymentAggregate payment = TestData.Payment();

    public PaymentRaceTest()
    {
        // The page lapsed past the grace with no transaction: the job closes it.
        payment.SetInitiateResponse(new Dictionary<string, string?> { ["redirectUrl"] = "https://checkout.wompi.co/p/" }, SystemClock.Instance.GetCurrentInstant() - Duration.FromTimeSpan(Grace) - Duration.FromSeconds(1));
        payment.CreatedAt = SystemClock.Instance.GetCurrentInstant() - Duration.FromDays(2);

        store.Add(payment);

        providers.Setup(x => x.GetCollect("Wompi")).Returns(collect.Object);
        resolver.Setup(x => x.ForPaymentAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Credentials());
        collect.Setup(x => x.GetOutcomeAsync(It.IsAny<PaymentAggregate>(), It.IsAny<MerchantCredentials>(), It.IsAny<CancellationToken>())).ReturnsAsync((CollectOutcome?)null);
        pubsub.Setup(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>())).Callback<IReadOnlyList<IDomainEvent>, CancellationToken>((events, _) => published.AddRange(events)).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Refresh_ApprovedWhileClosingTheLapsedPage_KeepsTheApproval()
    {
        // Arrange
        store.BeforeNextSave(ApproveAsync);

        // Act
        await Refresh();

        // Assert
        Assert.Equal((PaymentStatus.Succeeded, false), (store.Find(payment.Id)!.Status, store.Find(payment.Id)!.ApprovedLate));
    }

    [Fact]
    public async Task Refresh_ApprovedWhileClosingTheLapsedPage_TellsTheModuleOnlyTheApproval()
    {
        // Arrange
        store.BeforeNextSave(ApproveAsync);

        // Act
        await Refresh();

        // Assert
        Assert.Equal([PaymentStatus.Succeeded], published.OfType<PaymentResponseAssociatedDomainEvent>().Select(x => x.Status));
    }

    [Fact]
    public async Task Sweep_ApprovedWhileClosingAStrandedPayment_KeepsTheApproval()
    {
        // Arrange
        store.Repository
             .Setup(x => x.GetInProgressOlderThanAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(() => [store.Find(payment.Id)!]);
        store.BeforeNextSave(ApproveAsync);

        // Act
        await new ExpireStalePaymentsCommandHandler(store.Repository.Object, pubsub.Object, Mock.Of<ILogger<ExpireStalePaymentsCommandHandler>>()).Handle(new ExpireStalePaymentsCommand(Duration.FromDays(1), 500), CancellationToken.None);

        // Assert
        Assert.Equal((PaymentStatus.Succeeded, 1), (store.Find(payment.Id)!.Status, published.OfType<PaymentResponseAssociatedDomainEvent>().Count()));
    }

    [Fact]
    public async Task Approval_AfterThePlatformClosedTheLapsedPage_IsKeptAsALateApproval()
    {
        // Arrange
        await Refresh();

        // Act
        await ApproveAsync();

        // Assert
        Assert.Equal((PaymentStatus.Succeeded, true), (store.Find(payment.Id)!.Status, store.Find(payment.Id)!.ApprovedLate));
    }

    /// <summary>
    /// The provider's webhook, approving the payment.
    /// </summary>
    private Task ApproveAsync()
    {
        var handler = new UpdateStatusCommandHandler(store.Repository.Object, Mock.Of<IUserContext>(), pubsub.Object, Mock.Of<ILiveChannelGrpc>(), Mock.Of<IProviderFeeCalculator>());

        return handler.Handle(new UpdateStatusCommand(payment.Id, PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "tx-665", Money.FromLong(119_000L, "COP"), "NEQUI"), CancellationToken.None);
    }

    private Task<PaymentStatusDto> Refresh()
    {
        var options = Options.Create(new PaymentProvidersOptions { CheckoutLifetime = TimeSpan.FromMinutes(15), CheckoutExpirationGrace = Grace, DefaultReturnUrl = new Uri("https://www.example.com/return"), EventsUrl = new Uri("https://www.example.com/notify/wompi") });

        var handler = new RefreshPaymentStatusCommandHandler(store.Repository.Object, Mock.Of<IUserContext>(), providers.Object, resolver.Object, Mock.Of<IMerchantAccountHealth>(), Mock.Of<IMediator>(), pubsub.Object, options, Mock.Of<ILogger<RefreshPaymentStatusCommandHandler>>());

        return handler.Handle(new RefreshPaymentStatusCommand(payment.Id), CancellationToken.None);
    }
}
