using System.Linq;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ValidateMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.MerchantAccount;

/// <summary>
/// A collection account is invalidated only when the provider rejects its keys repeatedly or when the stored keys are
/// checked again and fail; never by a single transient error.
/// </summary>
public class MerchantAccountHealthTest
{
    private readonly Mock<IMerchantAccountRepository> repository = new();
    private readonly Mock<IMerchantSecretsCache> cache = new();
    private readonly Mock<IPubSub> pubsub = new();
    private MerchantAccountAggregate stored = TestData.ActiveAccount();

    public MerchantAccountHealthTest()
    {
        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(() => stored);
        repository
            .Setup(x => x.UpdateWithRetryAsync(It.IsAny<Func<Task<MerchantAccountAggregate>>>(), It.IsAny<Func<MerchantAccountAggregate, bool>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<MerchantAccountAggregate>>, Func<MerchantAccountAggregate, bool>, CancellationToken>(async (load, mutate, _) =>
            {
                var entity = await load();

                return mutate(entity);
            });
    }

    [Fact]
    public async Task ReportRejection_BelowTheThreshold_KeepsTheAccountActive()
    {
        // Act
        await Report(times: 2);

        // Assert
        Assert.Equal((MerchantAccountStatus.Active, 2), (stored.Status, stored.ConsecutiveRejections));
        pubsub.Verify(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReportRejection_AtTheThreshold_InvalidatesAndTellsTheAdministration()
    {
        // Act
        await Report(times: 3);

        // Assert
        Assert.Equal(MerchantAccountStatus.Invalid, stored.Status);
        pubsub.Verify(x => x.PublishAsync(It.Is<IReadOnlyList<IDomainEvent>>(events => events.OfType<MerchantAccountInvalidatedDomainEvent>().Any()), It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(x => x.Forget(stored.Id), Times.Once);
    }

    [Fact]
    public async Task ReportAcceptance_AfterTransientRejections_StartsCountingAgain()
    {
        // Arrange
        await Report(times: 2);

        // Act
        await Health().ReportAcceptanceAsync(TestData.Payment(), TestData.Credentials() with { PendingRejections = 2 }, CancellationToken.None);
        await Report(times: 2);

        // Assert
        Assert.Equal(MerchantAccountStatus.Active, stored.Status);
    }

    [Fact]
    public async Task ReportAcceptance_NoPendingRejections_DoesNotWrite()
    {
        // Act: the steady state, one per paid event, must not cost a write.
        await Health().ReportAcceptanceAsync(TestData.Payment(), TestData.Credentials(), CancellationToken.None);

        // Assert
        repository.Verify(x => x.UpdateWithRetryAsync(It.IsAny<Func<Task<MerchantAccountAggregate>>>(), It.IsAny<Func<MerchantAccountAggregate, bool>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReportRejection_PlatformPayment_TouchesNoTenantAccount()
    {
        // Act
        await Health().ReportRejectionAsync(TestData.Payment(platformAccount: true), "The private key was rejected.", CancellationToken.None);

        // Assert
        repository.Verify(x => x.GetByTenantAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Validate_StoredKeysRejected_InvalidatesTheAccount()
    {
        // Act
        var dto = await Validate(new MerchantValidation(false, MerchantAccountEnvironment.Sandbox, null, "Wompi does not accept the private key."));

        // Assert
        Assert.Equal((MerchantAccountStatus.Invalid, "Wompi does not accept the private key."), (dto.Status, dto.InvalidReason));
    }

    [Fact]
    public async Task Validate_InvalidAccountWithWorkingKeys_IsActiveAgain()
    {
        // Arrange
        stored.Invalidate("rejected", TestData.User);
        stored.GetAndClearEvents();

        // Act
        var dto = await Validate(new MerchantValidation(true, MerchantAccountEnvironment.Sandbox, "Comercio SAS", null));

        // Assert
        Assert.Equal((MerchantAccountStatus.Active, (string?)null), (dto.Status, dto.InvalidReason));
        pubsub.Verify(x => x.PublishAsync(It.Is<IReadOnlyList<IDomainEvent>>(events => events.OfType<MerchantAccountRevalidatedDomainEvent>().Any()), It.IsAny<CancellationToken>()), Times.Once);
    }

    private async Task Report(int times)
    {
        for (var i = 0; i < times; i++)
        {
            await Health().ReportRejectionAsync(TestData.Payment(), "The private key was rejected.", CancellationToken.None);
        }
    }

    private MerchantAccountHealth Health() => new(repository.Object, cache.Object, pubsub.Object, Options(), Mock.Of<ILogger<MerchantAccountHealth>>());

    private Task<CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects.MerchantAccountDto> Validate(MerchantValidation validation)
    {
        var cipher = new Mock<IMerchantSecretCipher>();
        cipher.Setup(x => x.OpenAsync(TestData.Tenant, It.IsAny<CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects.SealedSecrets>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Secrets());

        var validator = new Mock<IMerchantCredentialsProvider>();
        validator.Setup(x => x.ValidateAsync("pub_test_tenant", It.IsAny<MerchantSecrets>(), It.IsAny<CancellationToken>())).ReturnsAsync(validation);

        var providers = new Mock<IPaymentProviderFactory>();
        providers.Setup(x => x.GetCredentials("Wompi")).Returns(validator.Object);

        var user = new Mock<IUserContext>();
        user.SetupGet(x => x.Tenant).Returns(TestData.Tenant);
        user.SetupGet(x => x.IdUser).Returns(TestData.User);

        var handler = new ValidateMerchantAccountCommandHandler(repository.Object, cipher.Object, cache.Object, providers.Object, user.Object, pubsub.Object, Options());

        return handler.Handle(new ValidateMerchantAccountCommand(), CancellationToken.None);
    }

    private static IOptions<PaymentProvidersOptions> Options() => Microsoft.Extensions.Options.Options.Create(new PaymentProvidersOptions
    {
        InvalidateAfterRejections = 3,
        DefaultReturnUrl = new Uri("https://www.example.com/return"),
        EventsUrl = new Uri("https://www.example.com/notify/wompi")
    });
}
