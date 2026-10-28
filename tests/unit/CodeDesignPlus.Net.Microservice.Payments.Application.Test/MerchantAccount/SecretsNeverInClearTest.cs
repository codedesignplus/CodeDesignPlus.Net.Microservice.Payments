using System.Linq;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ConnectMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.Mongo.Extensions;
using CodeDesignPlus.Net.ValueObjects.Location;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.MerchantAccount;

/// <summary>
/// A secret pasted by the tenant never reaches the document Mongo stores nor a log line in clear: only the Vault's
/// ciphertext and the last four characters.
/// </summary>
public class SecretsNeverInClearTest
{
    // Probes: long and unmistakable, so finding one anywhere is the leak. Their last four characters are the hints.
    private const string PrivateKey = "prv_test_PRIVATE-PROBE-660-zz11";
    private const string EventsSecret = "test_events_EVENTS-PROBE-660-zz22";
    private const string IntegritySecret = "test_integrity_INTEGRITY-PROBE-660-zz33";
    private const string PayoutsApiKey = "PAYOUTS-APIKEY-PROBE-660-zz44";
    private const string PayoutsPrincipal = "PAYOUTS-PRINCIPAL-PROBE-660-zz55";

    private static readonly string[] Probes = ["PRIVATE-PROBE", "EVENTS-PROBE", "INTEGRITY-PROBE", "APIKEY-PROBE", "PRINCIPAL-PROBE"];

    private readonly CapturingLogger<MerchantAccountConnector> connectorLog = new();
    private readonly Mock<IMerchantAccountRepository> repository = new();
    private readonly Mock<IPaymentProviderFactory> providers = new();
    private readonly Mock<IMerchantCredentialsProvider> validator = new();
    private readonly Mock<IPayoutProvider> payouts = new();
    private MerchantAccountAggregate? stored;

    public SecretsNeverInClearTest()
    {
        MongoSerializerRegistration.RegisterSerializers();

        providers.Setup(x => x.GetCredentials("Wompi")).Returns(validator.Object);
        providers.Setup(x => x.GetPayouts("Wompi")).Returns(payouts.Object);

        repository.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(() => stored);
        repository
            .Setup(x => x.UpdateWithRetryAsync(It.IsAny<Func<Task<MerchantAccountAggregate>>>(), It.IsAny<Func<MerchantAccountAggregate, bool>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<MerchantAccountAggregate>>, Func<MerchantAccountAggregate, bool>, CancellationToken>(async (load, mutate, _) =>
            {
                var entity = await load();

                mutate(entity);
                stored = entity;

                return true;
            });
    }

    [Fact]
    public async Task Connect_StoredDocument_HasNoSecretInClear()
    {
        // Arrange
        Validates(true);

        // Act
        await Handler().Handle(Command(), CancellationToken.None);

        // Assert
        var document = stored!.ToBsonDocument().ToJson();
        Assert.False(Probes.Any(probe => document.Contains(probe, StringComparison.Ordinal)), document);
    }

    [Fact]
    public async Task Connect_StoredDocument_KeepsTheCiphertextAndTheHints()
    {
        // Arrange: the test of the test; an empty document would pass the one above.
        Validates(true);

        // Act
        await Handler().Handle(Command(), CancellationToken.None);

        // Assert
        var document = stored!.ToBsonDocument().ToJson();
        Assert.True(document.Contains("vault:v1:", StringComparison.Ordinal) && document.Contains("zz11", StringComparison.Ordinal), document);
    }

    [Fact]
    public async Task Connect_RejectedKeys_LogNoSecretInClear()
    {
        // Arrange
        Validates(false);

        // Act
        await Assert.ThrowsAsync<CodeDesignPlusException>(() => Handler().Handle(Command(), CancellationToken.None));

        // Assert
        Assert.False(connectorLog.Lines.Count == 0 || Probes.Any(probe => connectorLog.Lines.Any(line => line.Contains(probe, StringComparison.Ordinal))), string.Join("\n", connectorLog.Lines));
    }

    private void Validates(bool isValid)
    {
        // The reason echoes nothing the tenant pasted, as the real adapter does.
        validator.Setup(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<MerchantSecrets>(), It.IsAny<CancellationToken>())).ReturnsAsync(new MerchantValidation(isValid, MerchantAccountEnvironment.Sandbox, "Comercio SAS", isValid ? null : "Wompi does not accept the private key."));
    }

    private ConnectMerchantAccountCommandHandler Handler()
    {
        var options = Options.Create(new PaymentProvidersOptions
        {
            Routing = new Dictionary<string, string> { ["CO"] = "Wompi" },
            DefaultReturnUrl = new Uri("https://www.example.com/return"),
            EventsUrl = new Uri("https://www.example.com/notify/wompi")
        });

        var connector = new MerchantAccountConnector(providers.Object, new FakeCipher(), options, connectorLog);

        var user = new Mock<IUserContext>();
        user.SetupGet(x => x.Tenant).Returns(TestData.Tenant);
        user.SetupGet(x => x.IdUser).Returns(TestData.User);

        var tenant = new Mock<Security.Abstractions.ITenant>();
        tenant.SetupGet(x => x.Country).Returns(Country.Create(Guid.NewGuid(), "Colombia", "CO", "COL", 170, "+57", "America/Bogota", CodeDesignPlus.Net.ValueObjects.Financial.Currency.Create(Guid.NewGuid(), "Peso colombiano", "COP", "$", 2, 170)));

        return new ConnectMerchantAccountCommandHandler(repository.Object, connector, user.Object, tenant.Object, Mock.Of<IPubSub>(), Mock.Of<IMerchantSecretsCache>(), options);
    }

    private static ConnectMerchantAccountCommand Command() => new("pub_test_tenant", PrivateKey, EventsSecret, IntegritySecret, PayoutsPrincipal, PayoutsApiKey, "wompi-account-1");

    /// <summary>
    /// Stands for the Vault: the ciphertext is opaque and does not contain the secret.
    /// </summary>
    private sealed class FakeCipher : IMerchantSecretCipher
    {
        public Task<SealedSecrets> SealAsync(Guid tenant, MerchantSecrets secrets, CancellationToken cancellationToken)
        {
            static string Seal(string? value) => value is null ? null! : "vault:v1:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));

            return Task.FromResult(new SealedSecrets("transit-key", Seal(secrets.PrivateKey), Seal(secrets.EventsSecret), Seal(secrets.IntegritySecret), secrets.PayoutsPrincipalId is null ? null : Seal(secrets.PayoutsPrincipalId), secrets.PayoutsApiKey is null ? null : Seal(secrets.PayoutsApiKey)));
        }

        public Task<MerchantSecrets> OpenAsync(Guid tenant, SealedSecrets secrets, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    /// <summary>
    /// Keeps every formatted line and every structured value, which is what reaches the observability backend.
    /// </summary>
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Lines { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Lines.Add(formatter(state, exception));

            if (state is IEnumerable<KeyValuePair<string, object?>> values)
            {
                Lines.AddRange(values.Select(x => $"{x.Key}={x.Value}"));
            }
        }
    }
}
