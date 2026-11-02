using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentOutcome;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using Xunit;
using AppErrors = CodeDesignPlus.Net.Microservice.Payments.Application.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Queries.GetPaymentOutcome;

/// <summary>
/// The screen of the payer reads why a payment did not go through, and only of a payment it may read (pendings/622).
/// </summary>
public class GetPaymentOutcomeQueryHandlerTest
{
    private static readonly Guid PaymentId = Guid.Parse("aa000000-0000-4000-8000-000000000001");
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    private static readonly Guid Creator = Guid.Parse("cc000000-0000-4000-8000-000000000003");
    private static readonly Guid OtherTenant = Guid.Parse("dd000000-0000-4000-8000-000000000004");
    private static readonly Guid OtherUser = Guid.Parse("ee000000-0000-4000-8000-000000000005");

    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<IUserContext> user = new();

    [Fact]
    public async Task Handle_DeclinedPaymentOfTheTenant_ReturnsTheReasonAndTheBankMessage()
    {
        // Arrange
        Session(Tenant, OtherUser);
        Stored(Declined(Tenant));

        // Act
        var outcome = await Handler().Handle(new GetPaymentOutcomeQuery(PaymentId), CancellationToken.None);

        // Assert
        Assert.Equal((PaymentId, PaymentStatus.Failed, PaymentFailureReason.Declined, "Fondos insuficientes"), (outcome.Id, outcome.Status, outcome.FailureReason, outcome.ProviderMessage));
    }

    [Fact]
    public async Task Handle_PlatformPaymentStartedByTheUser_IsReadFromAnyTenant()
    {
        // Arrange: a license is read by the administrator who bought it, whatever tenant the session carries.
        Session(OtherTenant, Creator);
        Stored(Declined(null, platformAccount: true));

        // Act
        var outcome = await Handler().Handle(new GetPaymentOutcomeQuery(PaymentId), CancellationToken.None);

        // Assert
        Assert.Equal(PaymentFailureReason.Declined, outcome.FailureReason);
    }

    [Fact]
    public async Task Handle_PaymentOfAnotherTenant_AnswersLikeOneThatDoesNotExist()
    {
        // Arrange
        Session(OtherTenant, OtherUser);
        Stored(Declined(Tenant));

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Handler().Handle(new GetPaymentOutcomeQuery(PaymentId), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.PaymentNotFound.GetCode(), exception.Code);
    }

    [Fact]
    public async Task Handle_UnknownPayment_IsNotFound()
    {
        // Arrange
        Session(Tenant, Creator);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Handler().Handle(new GetPaymentOutcomeQuery(PaymentId), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.PaymentNotFound.GetCode(), exception.Code);
    }

    private GetPaymentOutcomeQueryHandler Handler() => new(repository.Object, user.Object);

    private void Session(Guid tenant, Guid idUser)
    {
        user.SetupGet(x => x.Tenant).Returns(tenant);
        user.SetupGet(x => x.IdUser).Returns(idUser);
    }

    private void Stored(PaymentAggregate payment)
    {
        repository.Setup(x => x.FindAsync<PaymentAggregate>(PaymentId, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
    }

    private static PaymentAggregate Declined(Guid? tenant, bool platformAccount = false)
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");
        var buyer = Buyer.CreateWithoutShipping(Guid.NewGuid(), "Ana Restrepo", "3001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");
        var module = platformAccount ? PaymentAggregate.LicensesModule : "Invoicing";

        var payment = PaymentAggregate.Create(PaymentId, module, Guid.NewGuid(), subTotal, tax, subTotal + tax, buyer, "Cuota", "Wompi", platformAccount, tenant, Creator);

        payment.SetFinalResponse(PaymentStatus.Failed, new Dictionary<string, string?> { ["status"] = "DECLINED" }, "tx-1", failureReason: PaymentFailureReason.Declined, providerMessage: "Fondos insuficientes");

        return payment;
    }
}
