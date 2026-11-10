using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.RefreshPaymentStatus;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Commands.RefreshPaymentStatus;

/// <summary>
/// Un cobro cuya página de pago venció sin transacción en el proveedor se cierra como vencido al refrescarlo (pendings/622).
/// </summary>
/// <remarks>
/// En Las Acacias, el 2-nov-2026, una reserva de parqueadero abrió Wompi a las 20:29:33 con una página que vencía a las
/// 20:44:33, y se cerró sin pagar. A las 20:50 el cobro seguía en curso: la conciliación le preguntaba a Wompi cada diez
/// minutos, Wompi no tenía transacción y nada lo cerraba hasta el barrido diario, un día después.
/// </remarks>
public class RefreshPaymentStatusCommandHandlerTest
{
    private static readonly TimeSpan CheckoutLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan Grace = TimeSpan.FromMinutes(5);

    private readonly PaymentAggregate payment = TestData.Payment();
    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<ICollectProvider> collect = new();
    private readonly Mock<IPaymentProviderFactory> providers = new();
    private readonly Mock<IMerchantCredentialsResolver> resolver = new();
    private readonly Mock<IMediator> mediator = new();
    private readonly Mock<IMerchantAccountHealth> health = new();
    private readonly Mock<IPubSub> pubsub = new();
    private readonly List<IDomainEvent> published = [];

    public RefreshPaymentStatusCommandHandlerTest()
    {
        payment.GetAndClearEvents();

        providers.Setup(x => x.GetCollect("Wompi")).Returns(collect.Object);
        repository.Setup(x => x.FindAsync<PaymentAggregate>(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        repository.ReappliesInPlace();
        resolver.Setup(x => x.ForPaymentAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Credentials());
        collect.Setup(x => x.GetOutcomeAsync(payment, It.IsAny<MerchantCredentials>(), It.IsAny<CancellationToken>())).ReturnsAsync((CollectOutcome?)null);
        pubsub.Setup(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>())).Callback<IReadOnlyList<IDomainEvent>, CancellationToken>((events, _) => published.AddRange(events)).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_PageExpiredWithoutTransaction_ExpiresThePayment()
    {
        // Arrange
        ExpiresAgo(Grace + TimeSpan.FromSeconds(1));

        // Act
        var result = await Handle();

        // Assert
        Assert.Equal(PaymentStatus.Expired, result.Status);
    }

    [Fact]
    public async Task Handle_PageExpiredWithoutTransaction_TellsTheModuleThatAskedForIt()
    {
        // Arrange
        ExpiresAgo(Grace + TimeSpan.FromSeconds(1));

        // Act
        await Handle();

        // Assert
        Assert.Contains(published.OfType<PaymentResponseAssociatedDomainEvent>(), x => x.AggregateId == payment.Id && x.Status == PaymentStatus.Expired);
    }

    [Fact]
    public async Task Handle_PageExpiredWithoutTransaction_SavesThePaymentBeforePublishing()
    {
        // Arrange
        var order = new List<string>();
        ExpiresAgo(Grace + TimeSpan.FromSeconds(1));
        repository.ReappliesInPlace(_ => order.Add("update"));
        pubsub.Setup(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>())).Callback(() => order.Add("publish")).Returns(Task.CompletedTask);

        // Act
        await Handle();

        // Assert
        Assert.Equal(["update", "publish"], order);
    }

    [Fact]
    public async Task Handle_PageStillOpenWithoutTransaction_LeavesThePaymentInProgress()
    {
        // Arrange: el pagador puede seguir en la página, dentro del margen.
        ExpiresAgo(Grace - TimeSpan.FromSeconds(5));

        // Act
        var result = await Handle();

        // Assert
        Assert.Equal(PaymentStatus.Initiated, result.Status);
    }

    [Fact]
    public async Task Handle_PageExpiredWithPendingTransaction_LeavesThePaymentInProgress()
    {
        // Arrange: una transacción iniciada antes de que venciera la página todavía puede aprobarse.
        ExpiresAgo(Grace + TimeSpan.FromMinutes(5));
        collect.Setup(x => x.GetOutcomeAsync(payment, It.IsAny<MerchantCredentials>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CollectOutcome(PaymentStatus.Pending, "tx-1", null, "PSE", new Dictionary<string, string?> { ["status"] = "PENDING" }));

        // Act
        var result = await Handle();

        // Assert
        Assert.Equal(PaymentStatus.Initiated, result.Status);
    }

    /// <summary>
    /// A payment closed by asking the provider keeps the card type too, the same as one closed by the webhook (Kappali
    /// pendings/661).
    /// </summary>
    [Fact]
    public async Task Handle_ApprovedDebitCard_AppliesTheCardType()
    {
        // Arrange
        ExpiresAgo(Grace + TimeSpan.FromMinutes(5));
        collect.Setup(x => x.GetOutcomeAsync(payment, It.IsAny<MerchantCredentials>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CollectOutcome(PaymentStatus.Succeeded, "tx-1", Money.FromLong(119_000L, "COP"), "CARD", new Dictionary<string, string?> { ["status"] = "APPROVED" }) { CardType = "DEBIT" });

        // Act
        await Handle();

        // Assert
        mediator.Verify(x => x.Send(It.Is<UpdateStatusCommand>(c => c.CardType == "DEBIT"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PageExpiredWithApprovedTransaction_AppliesTheProviderAnswer()
    {
        // Arrange
        ExpiresAgo(Grace + TimeSpan.FromMinutes(5));
        collect.Setup(x => x.GetOutcomeAsync(payment, It.IsAny<MerchantCredentials>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CollectOutcome(PaymentStatus.Succeeded, "tx-1", Money.FromLong(119_000L, "COP"), "CARD", new Dictionary<string, string?> { ["status"] = "APPROVED" }));

        // Act
        await Handle();

        // Assert
        mediator.Verify(x => x.Send(It.Is<UpdateStatusCommand>(c => c.Id == payment.Id && c.Status == PaymentStatus.Succeeded), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AccountWithoutKeys_DoesNotExpireWithoutAskingTheProvider()
    {
        // Arrange
        ExpiresAgo(Grace + TimeSpan.FromMinutes(5));
        resolver.Setup(x => x.ForPaymentAsync(payment, It.IsAny<CancellationToken>())).ReturnsAsync((MerchantCredentials?)null);

        // Act
        var result = await Handle();

        // Assert
        Assert.Equal(PaymentStatus.Initiated, result.Status);
    }

    [Fact]
    public async Task Handle_PageExpiresLaterThanItsCreationSuggests_WaitsForTheExpirationSentToTheProvider()
    {
        // Arrange: creado hace 25 minutos, pero la página que se le envió a Wompi vence hace solo 2; el margen no ha pasado.
        CreatedAgo(TimeSpan.FromMinutes(25));
        ExpiresAgo(TimeSpan.FromMinutes(2));

        // Act
        var result = await Handle();

        // Assert
        Assert.Equal(PaymentStatus.Initiated, result.Status);
    }

    [Fact]
    public async Task Handle_PageSentToTheProviderExpiredPastTheGrace_ExpiresThePayment()
    {
        // Arrange: recién creado según CreatedAt, pero el vencimiento guardado ya pasó con su margen.
        CreatedAgo(TimeSpan.FromMinutes(1));
        ExpiresAgo(Grace + TimeSpan.FromSeconds(1));

        // Act
        var result = await Handle();

        // Assert
        Assert.Equal(PaymentStatus.Expired, result.Status);
    }

    [Fact]
    public async Task Handle_PaymentWithoutTheExpirationOfItsPage_IsNotExpiredAsAbandoned()
    {
        // Arrange: creado hace una hora y sin transacción, pero sin el vencimiento que se le envió a la página; no se estima
        // uno con la creación (pendings/622).
        CreatedAgo(TimeSpan.FromHours(1));

        // Act
        var result = await Handle();

        // Assert
        Assert.Equal(PaymentStatus.Initiated, result.Status);
    }

    [Fact]
    public async Task Handle_FailedAttemptFound_ClosesThePaymentRightAway()
    {
        // Arrange: Wompi rechazó el intento hace un minuto; la conciliación lo cierra sin esperar a que el pagador reintente.
        ExpiresAgo(-TimeSpan.FromMinutes(10));
        collect.Setup(x => x.GetOutcomeAsync(payment, It.IsAny<MerchantCredentials>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CollectOutcome(PaymentStatus.Failed, "tx-1", null, "CARD", new Dictionary<string, string?> { ["status"] = "DECLINED" }));

        // Act
        await Handle();

        // Assert
        mediator.Verify(x => x.Send(It.Is<UpdateStatusCommand>(c => c.Id == payment.Id && c.Status == PaymentStatus.Failed), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Options_Default_GraceIsFiveMinutes()
    {
        // Act
        var options = new PaymentProvidersOptions();

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(5), options.CheckoutExpirationGrace);
    }

    private void ExpiresAgo(TimeSpan age)
    {
        payment.SetInitiateResponse(new Dictionary<string, string?> { ["redirectUrl"] = "https://checkout.wompi.co/p/" }, SystemClock.Instance.GetCurrentInstant() - Duration.FromTimeSpan(age));
        payment.GetAndClearEvents();
    }

    private void CreatedAgo(TimeSpan age) => payment.CreatedAt = SystemClock.Instance.GetCurrentInstant() - Duration.FromTimeSpan(age);

    private Task<PaymentStatusDto> Handle()
    {
        var options = Options.Create(new PaymentProvidersOptions { CheckoutLifetime = CheckoutLifetime, CheckoutExpirationGrace = Grace, DefaultReturnUrl = new Uri("https://www.example.com/return"), EventsUrl = new Uri("https://www.example.com/notify/wompi") });

        var handler = new RefreshPaymentStatusCommandHandler(repository.Object, Mock.Of<IUserContext>(), providers.Object, resolver.Object, health.Object, mediator.Object, pubsub.Object, options, Mock.Of<ILogger<RefreshPaymentStatusCommandHandler>>());

        return handler.Handle(new RefreshPaymentStatusCommand(payment.Id), CancellationToken.None);
    }
}
