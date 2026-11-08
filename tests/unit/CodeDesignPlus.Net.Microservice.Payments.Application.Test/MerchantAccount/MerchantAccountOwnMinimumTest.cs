using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.SetMerchantAccountMinimumAmount;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using CodeDesignPlus.Net.ValueObjects.Financial;
using DomainErrors = CodeDesignPlus.Net.Microservice.Payments.Domain.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.MerchantAccount;

/// <summary>
/// A property sets its own minimum to pay online on its collection account (pendings/660): it can raise the provider's
/// ($1.500 for Wompi), never lower it, and what is charged online is the larger of the two.
/// </summary>
public class MerchantAccountOwnMinimumTest
{
    private static readonly PaymentProvidersOptions ProviderOptions = new()
    {
        DefaultReturnUrl = new Uri("https://www.example.com/return"),
        EventsUrl = new Uri("https://www.example.com/notify/wompi"),
        MinimumAmounts = new() { ["Wompi"] = new() { ["COP"] = 150_000L } },
    };

    private readonly Mock<IMerchantAccountRepository> repository = new();
    private readonly Mock<IUserContext> user = new();
    private readonly Mock<IPubSub> pubsub = new();
    private readonly Mock<ICurrencyGrpc> currencies = new();
    private readonly List<IDomainEvent> published = [];
    private readonly MerchantAccountAggregate account = TestData.ActiveAccount();

    public MerchantAccountOwnMinimumTest()
    {
        user.SetupGet(x => x.Tenant).Returns(TestData.Tenant);
        user.SetupGet(x => x.IdUser).Returns(TestData.User);
        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        repository.Setup(x => x.UpdateWithRetryAsync(It.IsAny<Func<Task<MerchantAccountAggregate>>>(), It.IsAny<Func<MerchantAccountAggregate, bool>>(), It.IsAny<CancellationToken>()))
                  .Returns(async (Func<Task<MerchantAccountAggregate>> load, Func<MerchantAccountAggregate, bool> mutate, CancellationToken _) => mutate(await load()));
        currencies.Setup(x => x.GetCurrencyAsync(It.IsAny<Guid?>(), "COP", It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Currency.Create(Guid.NewGuid(), "Peso colombiano", "COP", "$", 2, 170));
        pubsub.Setup(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>()))
              .Callback<IReadOnlyList<IDomainEvent>, CancellationToken>((events, _) => published.AddRange(events))
              .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Set_AboveTheProviderMinimum_IsTheEffectiveMinimum()
    {
        // Act
        var dto = await Handle(new MoneyInput(5_000m, "COP"));

        // Assert
        Assert.Equal((500_000L, 500_000L, 150_000L), (Assert.Single(dto.MinimumAmounts).Amount, dto.MinimumAmount!.Amount, Assert.Single(dto.ProviderMinimumAmounts).Amount));
    }

    [Fact]
    public async Task Set_AboveTheProviderMinimum_AnnouncesIt()
    {
        // Act
        await Handle(new MoneyInput(5_000m, "COP"));

        // Assert
        Assert.Equal(500_000L, Assert.Single(published.OfType<MerchantAccountMinimumAmountChangedDomainEvent>()).MinimumAmount!.Amount);
    }

    [Fact]
    public async Task Set_BelowTheProviderMinimum_IsRefusedWithItsError()
    {
        // Act
        var error = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Handle(new MoneyInput(1_000m, "COP")));

        // Assert
        Assert.Equal((DomainErrors.MinimumAmountBelowProviderMinimum.GetCode(), (Money?)null), (error.Code, account.MinimumAmount));
    }

    [Fact]
    public async Task Set_ExactlyTheProviderMinimum_IsAccepted()
    {
        // Act
        var dto = await Handle(new MoneyInput(1_500m, "COP"));

        // Assert
        Assert.Equal(150_000L, dto.MinimumAmount!.Amount);
    }

    [Fact]
    public async Task Set_Null_ClearsItAndLeavesTheProviderMinimum()
    {
        // Arrange
        await Handle(new MoneyInput(5_000m, "COP"));

        // Act
        var dto = await Handle(null);

        // Assert
        Assert.Equal(((Money?)null, 150_000L), (dto.MinimumAmount, Assert.Single(dto.MinimumAmounts).Amount));
    }

    [Fact]
    public async Task Set_TheSameAgain_AnnouncesNothingNew()
    {
        // Arrange
        await Handle(new MoneyInput(5_000m, "COP"));

        // Act
        await Handle(new MoneyInput(5_000m, "COP"));

        // Assert
        Assert.Single(published.OfType<MerchantAccountMinimumAmountChangedDomainEvent>());
    }

    [Fact]
    public void Set_Zero_IsRefusedByTheAccount()
    {
        // Act
        var error = Assert.Throws<CodeDesignPlusException>(() => account.SetMinimumAmount(Money.FromLong(0, "COP"), null, TestData.User));

        // Assert
        Assert.Equal(DomainErrors.MinimumAmountMustBePositive.GetCode(), error.Code);
    }

    [Fact]
    public void Effective_OwnMinimumBelowTheProviderOne_KeepsTheProviderOne()
    {
        // Act
        var minimum = ProviderOptions.EffectiveMinimumFor("Wompi", "COP", Money.FromLong(100_000L, "COP"));

        // Assert
        Assert.Equal(150_000L, minimum!.Amount);
    }

    [Fact]
    public void From_AccountWithoutOwnMinimum_ShowsTheProviderOne()
    {
        // Act
        var dto = MerchantAccountDto.From(account, ProviderOptions);

        // Assert
        Assert.Equal(((Money?)null, 150_000L, 150_000L), (dto.MinimumAmount, Assert.Single(dto.MinimumAmounts).Amount, Assert.Single(dto.ProviderMinimumAmounts).Amount));
    }

    private Task<MerchantAccountDto> Handle(MoneyInput? minimum)
    {
        var handler = new SetMerchantAccountMinimumAmountCommandHandler(repository.Object, user.Object, pubsub.Object, currencies.Object, Microsoft.Extensions.Options.Options.Create(ProviderOptions));

        return handler.Handle(new SetMerchantAccountMinimumAmountCommand(minimum), CancellationToken.None);
    }
}
