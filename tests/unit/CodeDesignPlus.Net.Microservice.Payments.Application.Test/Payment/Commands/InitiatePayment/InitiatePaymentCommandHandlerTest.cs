using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.User;
using Microsoft.Extensions.Options;
using Xunit;
using AppErrors = CodeDesignPlus.Net.Microservice.Payments.Application.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Commands.InitiatePayment;

/// <summary>
/// Starting a payment always answers the provider's hosted page of the account that charges.
/// </summary>
public class InitiatePaymentCommandHandlerTest
{
    private static readonly Uri HostedPage = new("https://checkout.wompi.co/p/?reference=x");
    private static readonly Uri DefaultReturnUrl = new("https://www.example.com/payments/return");

    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<IUserContext> user = new();
    private readonly Mock<IPubSub> pubsub = new();
    private readonly Mock<IMerchantCredentialsResolver> resolver = new();
    private readonly Mock<IPaymentProviderFactory> providers = new();
    private readonly Mock<ICollectProvider> collect = new();

    public InitiatePaymentCommandHandlerTest()
    {
        user.SetupGet(x => x.Tenant).Returns(TestData.Tenant);
        user.SetupGet(x => x.IdUser).Returns(TestData.User);
        resolver.Setup(x => x.ForNewPaymentAsync(TestData.Tenant, false, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Credentials());
        collect.SetupGet(x => x.Code).Returns("Wompi");
        collect.Setup(x => x.CreateCheckoutAsync(It.IsAny<PaymentAggregate>(), It.IsAny<MerchantCredentials>(), It.IsAny<Uri>(), It.IsAny<Instant>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CheckoutSession(HostedPage));
        providers.Setup(x => x.GetCollect("Wompi")).Returns(collect.Object);
    }

    [Fact]
    public async Task Handle_ActiveAccount_AnswersRedirectToTheHostedPage()
    {
        // Act
        var response = await Handler().Handle(Command(), CancellationToken.None);

        // Assert
        Assert.Equal((true, NextActionType.Redirect, HostedPage.ToString()), (response.Success, response.NextAction, response.RedirectUrl));
    }

    [Fact]
    public async Task Handle_WithoutReturnUrl_UsesTheConfiguredOneWithThePaymentId()
    {
        // Arrange
        var command = Command();

        // Act
        await Handler().Handle(command, CancellationToken.None);

        // Assert
        collect.Verify(x => x.CreateCheckoutAsync(It.IsAny<PaymentAggregate>(), It.IsAny<MerchantCredentials>(), new Uri($"{DefaultReturnUrl}?paymentId={command.Id}"), It.IsAny<Instant>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithReturnUrl_UsesTheCallerOneWithThePaymentId()
    {
        // Arrange
        var command = Command(returnUrl: "https://www.example.com/booking/return");

        // Act
        await Handler().Handle(command, CancellationToken.None);

        // Assert
        collect.Verify(x => x.CreateCheckoutAsync(It.IsAny<PaymentAggregate>(), It.IsAny<MerchantCredentials>(), new Uri($"https://www.example.com/booking/return?paymentId={command.Id}"), It.IsAny<Instant>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProviderOtherThanTheAccountOne_ThrowsAndCreatesNothing()
    {
        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Handler().Handle(Command(provider: "MercadoPago"), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.PaymentProviderDoesNotMatchAccount.GetCode(), exception.Code);
        repository.Verify(x => x.CreateAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AccountNotActive_ThrowsAndCreatesNothing()
    {
        // Arrange
        resolver.Setup(x => x.ForNewPaymentAsync(TestData.Tenant, false, It.IsAny<CancellationToken>())).ThrowsAsync(new CodeDesignPlusException(Layer.Application, AppErrors.MerchantAccountIsNotActive.GetCode(), AppErrors.MerchantAccountIsNotActive.GetMessage()));

        // Act
        await Assert.ThrowsAsync<CodeDesignPlusException>(() => Handler().Handle(Command(), CancellationToken.None));

        // Assert
        repository.Verify(x => x.CreateAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Payment_IsStoredWithTheProviderOfTheAccount()
    {
        // Arrange
        PaymentAggregate? created = null;
        repository.Setup(x => x.CreateAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>())).Callback<PaymentAggregate, CancellationToken>((payment, _) => created = payment);

        // Act
        await Handler().Handle(Command(), CancellationToken.None);

        // Assert
        Assert.Equal(("Wompi", false, TestData.Tenant), (created!.Provider, created.PlatformAccount, created.Tenant));
    }

    [Fact]
    public async Task Handle_Payment_KeepsTheExpirationSentToTheHostedPage()
    {
        // Arrange
        Instant? sent = null;
        PaymentAggregate? updated = null;
        collect.Setup(x => x.CreateCheckoutAsync(It.IsAny<PaymentAggregate>(), It.IsAny<MerchantCredentials>(), It.IsAny<Uri>(), It.IsAny<Instant>(), It.IsAny<CancellationToken>())).Callback<PaymentAggregate, MerchantCredentials, Uri, Instant, CancellationToken>((_, _, _, expiresAt, _) => sent = expiresAt).ReturnsAsync(new CheckoutSession(HostedPage));
        repository.Setup(x => x.UpdateAsync(It.IsAny<PaymentAggregate>(), It.IsAny<CancellationToken>())).Callback<PaymentAggregate, CancellationToken>((payment, _) => updated = payment);

        // Act
        await Handler().Handle(Command(), CancellationToken.None);

        // Assert
        Assert.Equal(sent, updated!.CheckoutExpiresAt);
    }

    private InitiatePaymentCommandHandler Handler(long? wompiMinimumCop = null)
    {
        var minimums = wompiMinimumCop is { } minimum ? new Dictionary<string, Dictionary<string, long>> { ["Wompi"] = new() { ["COP"] = minimum } } : [];
        var options = Options.Create(new PaymentProvidersOptions { DefaultReturnUrl = DefaultReturnUrl, EventsUrl = new Uri("https://www.example.com/notify/wompi"), MinimumAmounts = minimums });

        return new InitiatePaymentCommandHandler(repository.Object, user.Object, pubsub.Object, resolver.Object, providers.Object, options);
    }

    /// <summary>
    /// Wompi's page refuses less than $1.500 before creating a transaction (pendings/665): the payment is refused here with its
    /// own error, before anything is stored or any page is asked for. The command's total is $1.190.
    /// </summary>
    [Fact]
    public async Task Handle_BelowTheProviderMinimum_ThrowsItsErrorAndCreatesNothing()
    {
        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Handler(wompiMinimumCop: 150_000L).Handle(Command(), CancellationToken.None));

        // Assert
        Assert.Equal((AppErrors.PaymentBelowProviderMinimum.GetCode(), 0), (exception.Code, repository.Invocations.Count(x => x.Method.Name == nameof(IPaymentRepository.CreateAsync)) + collect.Invocations.Count(x => x.Method.Name == nameof(ICollectProvider.CreateCheckoutAsync))));
    }

    [Fact]
    public async Task Handle_ExactlyTheProviderMinimum_AnswersTheHostedPage()
    {
        // Act
        var response = await Handler(wompiMinimumCop: 119_000L).Handle(Command(), CancellationToken.None);

        // Assert
        Assert.True(response.Success);
    }

    /// <summary>
    /// A property can raise the minimum on its collection account (pendings/660): its $2.000 applies over Wompi's $1.000 here.
    /// </summary>
    [Fact]
    public async Task Handle_BelowTheMinimumThePropertyRaised_ThrowsTheSameError()
    {
        // Arrange
        resolver.Setup(x => x.ForNewPaymentAsync(TestData.Tenant, false, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Credentials() with { MinimumAmount = CodeDesignPlus.Net.ValueObjects.Financial.Money.FromLong(200_000L, "COP") });

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => Handler(wompiMinimumCop: 100_000L).Handle(Command(), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.PaymentBelowProviderMinimum.GetCode(), exception.Code);
    }

    [Fact]
    public async Task Handle_AboveTheProviderMinimumWithoutOwnMinimum_AnswersTheHostedPage()
    {
        // Act
        var response = await Handler(wompiMinimumCop: 100_000L).Handle(Command(), CancellationToken.None);

        // Assert
        Assert.True(response.Success);
    }

    [Fact]
    public async Task Handle_MinimumOfAnotherCurrency_DoesNotApply()
    {
        // Arrange
        var minimums = new Dictionary<string, Dictionary<string, long>> { ["Wompi"] = new() { ["USD"] = 500_000L } };
        var handler = new InitiatePaymentCommandHandler(repository.Object, user.Object, pubsub.Object, resolver.Object, providers.Object, Options.Create(new PaymentProvidersOptions { DefaultReturnUrl = DefaultReturnUrl, EventsUrl = new Uri("https://www.example.com/notify/wompi"), MinimumAmounts = minimums }));

        // Act
        var response = await handler.Handle(Command(), CancellationToken.None);

        // Assert
        Assert.True(response.Success);
    }

    [Fact]
    public void Options_NegativeMinimum_IsRejectedAtStartUp()
    {
        // Arrange
        var options = new PaymentProvidersOptions { DefaultReturnUrl = DefaultReturnUrl, EventsUrl = new Uri("https://www.example.com/notify/wompi"), SecretsKeyName = "merchant-accounts", MinimumAmounts = new() { ["Wompi"] = new() { ["COP"] = -1 } } };

        // Act
        var problems = options.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(options)).ToList();

        // Assert
        Assert.Contains(problems, x => x.MemberNames.Contains(nameof(PaymentProvidersOptions.MinimumAmounts)));
    }

    private static InitiatePaymentCommand Command(string? provider = null, string? returnUrl = null)
    {
        var buyer = Buyer.CreateMinimal(TestData.User, "Ana Restrepo", "3001112233", "ana@example.com");

        return new InitiatePaymentCommand(Guid.NewGuid(), "Invoicing", Guid.NewGuid(), Amount.Create(100_000L, "COP"), Amount.Create(19_000L, "COP"), Amount.Create(119_000L, "COP"), "Cuota", buyer, provider, returnUrl, false);
    }
}
