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

    private InitiatePaymentCommandHandler Handler()
    {
        var options = Options.Create(new PaymentProvidersOptions { DefaultReturnUrl = DefaultReturnUrl, EventsUrl = new Uri("https://www.example.com/notify/wompi") });

        return new InitiatePaymentCommandHandler(repository.Object, user.Object, pubsub.Object, resolver.Object, providers.Object, options);
    }

    private static InitiatePaymentCommand Command(string? provider = null, string? returnUrl = null)
    {
        var buyer = Buyer.CreateMinimal(TestData.User, "Ana Restrepo", "3001112233", "ana@example.com");

        return new InitiatePaymentCommand(Guid.NewGuid(), "Invoicing", Guid.NewGuid(), Amount.Create(100_000L, "COP"), Amount.Create(19_000L, "COP"), Amount.Create(119_000L, "COP"), "Cuota", buyer, provider, returnUrl, false);
    }
}
