using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ReconcilePayments;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.RefreshPaymentStatus;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Commands.ReconcilePayments;

/// <summary>
/// The reconciliation closes a declined payment when its retry window ends, even if it is younger than the minimum age it
/// asks about (pendings/622).
/// </summary>
public class ReconcilePaymentsCommandHandlerTest
{
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");

    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<IMediator> mediator = new();

    [Fact]
    public async Task Handle_RetryWindowEnded_AsksTheProviderAboutThePayment()
    {
        // Arrange
        var payment = Declined();

        repository.Setup(x => x.GetInProgressOlderThanAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.GetRetryWindowsEndedAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([payment]);
        mediator.Setup(x => x.Send(It.IsAny<RefreshPaymentStatusCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(PaymentStatusDto.From(payment));

        var handler = new ReconcilePaymentsCommandHandler(repository.Object, mediator.Object, Options.Create(new PaymentProvidersOptions()), NullLogger<ReconcilePaymentsCommandHandler>.Instance);

        // Act
        await handler.Handle(new ReconcilePaymentsCommand(), CancellationToken.None);

        // Assert
        mediator.Verify(x => x.Send(It.Is<RefreshPaymentStatusCommand>(c => c.Id == payment.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PaymentInBothLists_IsAskedAboutOnce()
    {
        // Arrange
        var payment = Declined();

        repository.Setup(x => x.GetInProgressOlderThanAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([payment]);
        repository.Setup(x => x.GetRetryWindowsEndedAsync(It.IsAny<Instant>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([payment]);
        mediator.Setup(x => x.Send(It.IsAny<RefreshPaymentStatusCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(PaymentStatusDto.From(payment));

        var handler = new ReconcilePaymentsCommandHandler(repository.Object, mediator.Object, Options.Create(new PaymentProvidersOptions()), NullLogger<ReconcilePaymentsCommandHandler>.Instance);

        // Act
        await handler.Handle(new ReconcilePaymentsCommand(), CancellationToken.None);

        // Assert
        mediator.Verify(x => x.Send(It.IsAny<RefreshPaymentStatusCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static PaymentAggregate Declined()
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(0L, "COP");
        var buyer = Buyer.CreateWithoutShipping(Guid.NewGuid(), "Ana Restrepo", "3001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");
        var payment = PaymentAggregate.Create(Guid.NewGuid(), "CommonAreas", Guid.NewGuid(), subTotal, tax, subTotal + tax, buyer, "Reserva", "Wompi", false, Tenant, Guid.NewGuid());

        payment.DeclineAttempt(new Dictionary<string, string?> { ["status"] = "DECLINED" }, "tx-1", "CARD", PaymentFailureReason.Declined, null, SystemClock.Instance.GetCurrentInstant() - Duration.FromSeconds(5));

        return payment;
    }
}
