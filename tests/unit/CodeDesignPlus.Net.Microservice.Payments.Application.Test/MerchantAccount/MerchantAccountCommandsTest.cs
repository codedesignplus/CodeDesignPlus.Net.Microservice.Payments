using System.Linq;
using System.Reflection;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ConnectMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.CreatePendingMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.DisconnectMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Queries.GetMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Location;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using DomainErrors = CodeDesignPlus.Net.Microservice.Payments.Domain.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.MerchantAccount;

/// <summary>
/// The collection account is born with the tenant, is connected only once, and never answers a secret.
/// </summary>
public class MerchantAccountCommandsTest
{
    private static readonly Uri EventsUrl = new("https://www.example.com/notify/wompi");

    private readonly Mock<IMerchantAccountRepository> repository = new();
    private readonly Mock<IMerchantAccountConnector> connector = new();
    private readonly Mock<IUserContext> user = new();
    private readonly Mock<IPubSub> pubsub = new();
    private readonly Mock<IMerchantSecretsCache> cache = new();
    private MerchantAccountAggregate? stored;

    public MerchantAccountCommandsTest()
    {
        user.SetupGet(x => x.Tenant).Returns(TestData.Tenant);
        user.SetupGet(x => x.IdUser).Returns(TestData.User);

        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(() => stored);
        repository
            .Setup(x => x.UpdateWithRetryAsync(It.IsAny<Func<Task<MerchantAccountAggregate>>>(), It.IsAny<Func<MerchantAccountAggregate, bool>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<MerchantAccountAggregate>>, Func<MerchantAccountAggregate, bool>, CancellationToken>(async (load, mutate, _) =>
            {
                var entity = await load();

                if (entity is null || !mutate(entity))
                {
                    return false;
                }

                stored = entity;

                return true;
            });

        connector
            .Setup(x => x.ValidateAndStoreAsync(It.IsAny<MerchantAccountAggregate>(), It.IsAny<MerchantKeys>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MerchantConnection(MerchantAccountEnvironment.Sandbox, "pub_test_tenant", "Comercio SAS", new SealedSecrets("vault:v1:prv", "vault:v1:evt", "vault:v1:int", null, null), "1111", "2222", "3333", null, null));
    }

    [Fact]
    public async Task CreatePending_NewTenant_RoutesTheAccountByCountry()
    {
        // Act
        await new CreatePendingMerchantAccountCommandHandler(repository.Object, pubsub.Object, Options(), Mock.Of<ILogger<CreatePendingMerchantAccountCommandHandler>>())
            .Handle(new CreatePendingMerchantAccountCommand(TestData.Tenant, "CO", TestData.User), CancellationToken.None);

        // Assert
        Assert.Equal((MerchantAccountAggregate.IdFor(TestData.Tenant), MerchantAccountStatus.PendingConnection, "Wompi"), (stored!.Id, stored.Status, stored.Provider));
        pubsub.Verify(x => x.PublishAsync(It.Is<IReadOnlyList<IDomainEvent>>(events => events.OfType<MerchantAccountCreatedDomainEvent>().Any()), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Connect_AccountAlreadyConnected_ThrowsBeforeTouchingTheKeys()
    {
        // Arrange
        stored = TestData.ActiveAccount();

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => ConnectHandler().Handle(ConnectCommand(), CancellationToken.None));

        // Assert
        Assert.Equal(DomainErrors.MerchantAccountIsAlreadyConnected.GetCode(), exception.Code);
        connector.Verify(x => x.ValidateAndStoreAsync(It.IsAny<MerchantAccountAggregate>(), It.IsAny<MerchantKeys>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Connect_TenantCreatedBeforeAccountsExisted_CreatesAndConnectsIt()
    {
        // Arrange
        stored = null;

        // Act
        var dto = await ConnectHandler().Handle(ConnectCommand(), CancellationToken.None);

        // Assert
        Assert.Equal((MerchantAccountStatus.Active, "Wompi", "1111", EventsUrl.ToString()), (dto.Status, dto.Provider, dto.PrivateKeyHint, dto.EventsUrl));
    }

    [Fact]
    public async Task Disconnect_ConnectedAccount_ForgetsTheCiphertextAndTheHints()
    {
        // Arrange
        stored = TestData.ActiveAccount();

        // Act
        await new DisconnectMerchantAccountCommandHandler(repository.Object, cache.Object, user.Object, pubsub.Object).Handle(new DisconnectMerchantAccountCommand(), CancellationToken.None);

        // Assert
        cache.Verify(x => x.Forget(stored!.Id), Times.Once);
        Assert.Equal((MerchantAccountStatus.Disabled, (SealedSecrets?)null, (string?)null), (stored!.Status, stored.Secrets, stored.PrivateKeyHint));
    }

    [Fact]
    public async Task GetMerchantAccount_TenantWithoutAccount_IsPendingConnection()
    {
        // Arrange
        stored = null;

        // Act
        var dto = await QueryHandler(TenantIn("CO")).Handle(new GetMerchantAccountQuery(TestData.Tenant), CancellationToken.None);

        // Assert
        Assert.Equal((MerchantAccountStatus.PendingConnection, false), (dto.Status, dto.PayoutsEnabled));
    }

    [Fact]
    public async Task GetMerchantAccount_TenantWithoutAccountInARoutedCountry_AnswersItsProvider()
    {
        // Arrange: a tenant created before collection accounts existed, in a country that has a provider.
        stored = null;

        // Act
        var dto = await QueryHandler(TenantIn("CO")).Handle(new GetMerchantAccountQuery(TestData.Tenant), CancellationToken.None);

        // Assert
        Assert.Equal("Wompi", dto.Provider);
    }

    [Fact]
    public async Task GetMerchantAccount_TenantWithoutAccountInACountryWithoutProvider_AnswersNoProvider()
    {
        // Arrange
        stored = null;

        // Act
        var dto = await QueryHandler(TenantIn("PE")).Handle(new GetMerchantAccountQuery(TestData.Tenant), CancellationToken.None);

        // Assert
        Assert.Null(dto.Provider);
    }

    [Fact]
    public async Task GetMerchantAccount_CallWithoutTheTenantInContext_LoadsItsCountry()
    {
        // Arrange: a worker over gRPC sends the tenant only in the request.
        stored = null;
        var tenant = TenantIn("CO");
        tenant.SetupGet(x => x.IsLoaded).Returns(false);
        tenant.Setup(x => x.SetAsync(TestData.Tenant, It.IsAny<CancellationToken>())).Callback(() => tenant.SetupGet(x => x.IsLoaded).Returns(true)).Returns(Task.CompletedTask);

        // Act
        var dto = await QueryHandler(tenant).Handle(new GetMerchantAccountQuery(TestData.Tenant), CancellationToken.None);

        // Assert
        Assert.Equal("Wompi", dto.Provider);
        tenant.Verify(x => x.SetAsync(TestData.Tenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MerchantAccountDto_Properties_NameNoSecret()
    {
        // Arrange: only hints may leave; a property named after a secret without "Hint" would be the secret itself.
        var properties = typeof(MerchantAccountDto).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => x.Name);

        // Act
        var leaking = properties.Where(name => (name.Contains("Private", StringComparison.Ordinal) || name.Contains("Secret", StringComparison.Ordinal) || name.Contains("ApiKey", StringComparison.Ordinal)) && !name.EndsWith("Hint", StringComparison.Ordinal)).ToList();

        // Assert
        Assert.Empty(leaking);
    }

    private ConnectMerchantAccountCommandHandler ConnectHandler()
    {
        var tenant = new Mock<Security.Abstractions.ITenant>();
        tenant.SetupGet(x => x.Country).Returns(Country.Create(Guid.NewGuid(), "Colombia", "CO", "COL", 170, "+57", "America/Bogota", CodeDesignPlus.Net.ValueObjects.Financial.Currency.Create(Guid.NewGuid(), "Peso colombiano", "COP", "$", 2, 170)));

        return new ConnectMerchantAccountCommandHandler(repository.Object, connector.Object, user.Object, tenant.Object, pubsub.Object, cache.Object, Options());
    }

    private GetMerchantAccountQueryHandler QueryHandler(Mock<Security.Abstractions.ITenant> tenant) => new(repository.Object, user.Object, tenant.Object, Options());

    private static Mock<Security.Abstractions.ITenant> TenantIn(string alpha2)
    {
        var tenant = new Mock<Security.Abstractions.ITenant>();
        tenant.SetupGet(x => x.IsLoaded).Returns(true);
        tenant.SetupGet(x => x.Country).Returns(Country.Create(Guid.NewGuid(), alpha2, alpha2, alpha2 + "X", 170, "+57", "America/Bogota", CodeDesignPlus.Net.ValueObjects.Financial.Currency.Create(Guid.NewGuid(), "Peso colombiano", "COP", "$", 2, 170)));

        return tenant;
    }

    private static ConnectMerchantAccountCommand ConnectCommand() => new("pub_test_tenant", "prv_test_1111", "events_2222", "integrity_3333", null, null, null);

    private static IOptions<PaymentProvidersOptions> Options() => Microsoft.Extensions.Options.Options.Create(new PaymentProvidersOptions
    {
        Routing = new Dictionary<string, string> { ["CO"] = "Wompi" },
        DefaultReturnUrl = new Uri("https://www.example.com/return"),
        EventsUrl = EventsUrl
    });
}
