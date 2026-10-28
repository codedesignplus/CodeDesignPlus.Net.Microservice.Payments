using CodeDesignPlus.Net.Microservice.Payments.Application.Payout.Commands.RequestPayout;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payout.Queries.GetPayout;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Microsoft.Extensions.Logging;
using Xunit;
using AppErrors = CodeDesignPlus.Net.Microservice.Payments.Application.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payout;

/// <summary>
/// A payout request is registered once per id, only for an account that can pay third parties.
/// </summary>
public class RequestPayoutCommandHandlerTest
{
    private readonly Mock<IPayoutRepository> repository = new();
    private readonly Mock<IMerchantAccountRepository> accounts = new();
    private readonly Mock<IBeneficiaryRepository> beneficiaries = new();
    private readonly Mock<IPubSub> pubsub = new();
    private readonly Guid beneficiaryId = Guid.NewGuid();

    public RequestPayoutCommandHandlerTest()
    {
        accounts.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.ActiveAccount("wompi-account-1"));
        beneficiaries.Setup(x => x.FindAsync<BeneficiaryAggregate>(beneficiaryId, TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(Beneficiary());
        repository.Setup(x => x.CreateIfAbsentAsync(It.IsAny<PayoutAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Handle_AccountWithPayouts_RegistersItRequested()
    {
        // Act
        var payout = await Handler().Handle(Command(), CancellationToken.None);

        // Assert
        Assert.Equal(PayoutStatus.Requested, payout.Status);
        repository.Verify(x => x.CreateIfAbsentAsync(It.IsAny<PayoutAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AccountWithoutPayouts_ThrowsPayoutsAreNotEnabled()
    {
        // Arrange
        accounts.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.ActiveAccount());

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Handler().Handle(Command(), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.PayoutsAreNotEnabled.GetCode(), exception.Code);
    }

    [Fact]
    public async Task Handle_RepeatedId_AnswersTheRegisteredPayoutWithoutCreatingAnother()
    {
        // Arrange
        var command = Command();
        var registered = PayoutAggregate.Request(command.Id, command.Tenant, command.Module, command.ReferenceId, command.Purpose, command.BeneficiaryId, command.Amount, command.Description, command.RequestedBy);
        repository.Setup(x => x.FindAsync<PayoutAggregate>(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(registered);

        // Act
        var payout = await Handler().Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(command.Id, payout.Id);
        repository.Verify(x => x.CreateIfAbsentAsync(It.IsAny<PayoutAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPayout_OfAnotherTenant_IsNotFound()
    {
        // Arrange
        var command = Command();
        var registered = PayoutAggregate.Request(command.Id, command.Tenant, command.Module, command.ReferenceId, command.Purpose, command.BeneficiaryId, command.Amount, command.Description, command.RequestedBy);
        repository.Setup(x => x.FindAsync<PayoutAggregate>(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(registered);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => new GetPayoutQueryHandler(repository.Object).Handle(new GetPayoutQuery(command.Id, Guid.NewGuid()), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.PayoutNotFound.GetCode(), exception.Code);
    }

    private RequestPayoutCommandHandler Handler() => new(repository.Object, accounts.Object, beneficiaries.Object, pubsub.Object, Mock.Of<ILogger<RequestPayoutCommandHandler>>());

    private RequestPayoutCommand Command() => new(Guid.NewGuid(), TestData.Tenant, "Expenses", Guid.NewGuid(), PayoutPurpose.Expense, beneficiaryId, Money.FromLong(500_000L, "COP"), "Pago de aseo de octubre", TestData.User);

    private BeneficiaryAggregate Beneficiary() => BeneficiaryAggregate.Create(beneficiaryId, Guid.NewGuid(), "1007", "Bancolombia", AccountType.Savings, "123456789", "NIT", "900123456", "Aseo SAS", "CO", "COP", null, null, null, TestData.Tenant);
}
