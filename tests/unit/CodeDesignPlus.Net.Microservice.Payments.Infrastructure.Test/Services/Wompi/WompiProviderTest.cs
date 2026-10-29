using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.gRpc.Clients.Services.Currencies;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi.Models;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using Microsoft.AspNetCore.WebUtilities;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Wompi;

/// <summary>
/// The Wompi adapter against a fake Wompi: no test here reaches the sandbox.
/// </summary>
public class WompiProviderTest
{
    private static readonly Guid PaymentId = Guid.Parse("aa000000-0000-4000-8000-000000000001");

    private readonly Mock<IWompiClient> client = new();
    private readonly Mock<ICurrencyGrpc> currencies = new();

    private readonly MerchantCredentials credentials = new("Wompi", MerchantAccountEnvironment.Sandbox, "pub_test_tenant", new MerchantSecrets("prv_test_tenant", "test_events_secret", "test_integrity_secret", null, null), null);

    public WompiProviderTest()
    {
        UseDecimalDigits(2);
    }

    [Fact]
    public async Task CreateCheckoutAsync_Payment_SignsTheHostedPageWithTheAccountIntegritySecret()
    {
        // Arrange
        var expiresAt = Instant.FromUtc(2026, 10, 28, 20, 0);

        // Act
        var session = await Provider().CreateCheckoutAsync(Payment(), credentials, new Uri("https://www.example.com/return"), expiresAt, CancellationToken.None);

        // Assert
        var query = QueryHelpers.ParseQuery(session.RedirectUrl.Query);
        Assert.Equal(
            ("pub_test_tenant", "COP", "11900000", PaymentId.ToString(), "2026-10-28T20:00:00.000Z", "https://www.example.com/return", WompiKeys.IntegritySignature(PaymentId.ToString(), 11_900_000L, "COP", "2026-10-28T20:00:00.000Z", "test_integrity_secret")),
            (query["public-key"].ToString(), query["currency"].ToString(), query["amount-in-cents"].ToString(), query["reference"].ToString(), query["expiration-time"].ToString(), query["redirect-url"].ToString(), query["signature:integrity"].ToString()));
    }

    [Fact]
    public async Task CreateCheckoutAsync_CurrencyWithoutDecimals_ConvertsToCents()
    {
        // Arrange: a currency kept in whole units still goes to Wompi in cents.
        UseDecimalDigits(0);
        var payment = Payment(100_000L, 19_000L);

        // Act
        var session = await Provider().CreateCheckoutAsync(payment, credentials, new Uri("https://www.example.com/return"), Instant.FromUtc(2026, 10, 28, 20, 0), CancellationToken.None);

        // Assert
        Assert.Equal("11900000", QueryHelpers.ParseQuery(session.RedirectUrl.Query)["amount-in-cents"].ToString());
    }

    [Fact]
    public async Task CreateCheckoutAsync_BuyerWithDocument_PrefillsThePage()
    {
        // Act
        var session = await Provider().CreateCheckoutAsync(Payment(), credentials, new Uri("https://www.example.com/return"), Instant.FromUtc(2026, 10, 28, 20, 0), CancellationToken.None);

        // Assert
        var query = QueryHelpers.ParseQuery(session.RedirectUrl.Query);
        Assert.Equal(("ana@example.com", "Ana Restrepo", "3001112233", "1020304050", "CC"), (query["customer-data:email"].ToString(), query["customer-data:full-name"].ToString(), query["customer-data:phone-number"].ToString(), query["customer-data:legal-id"].ToString(), query["customer-data:legal-id-type"].ToString()));
    }

    [Fact]
    public async Task CreateCheckoutAsync_Url_CarriesNoSecret()
    {
        // Act
        var session = await Provider().CreateCheckoutAsync(Payment(), credentials, new Uri("https://www.example.com/return"), Instant.FromUtc(2026, 10, 28, 20, 0), CancellationToken.None);

        // Assert
        var url = session.RedirectUrl.ToString();
        Assert.False(url.Contains("test_integrity_secret", StringComparison.Ordinal) || url.Contains("prv_test_tenant", StringComparison.Ordinal) || url.Contains("test_events_secret", StringComparison.Ordinal), url);
    }

    [Theory]
    [InlineData("APPROVED", PaymentStatus.Succeeded)]
    [InlineData("DECLINED", PaymentStatus.Failed)]
    [InlineData("VOIDED", PaymentStatus.Failed)]
    [InlineData("ERROR", PaymentStatus.Failed)]
    [InlineData("PENDING", PaymentStatus.Pending)]
    public async Task ReadNotificationAsync_TransactionUpdated_NormalizesTheStatus(string wompiStatus, PaymentStatus expected)
    {
        // Act
        var notification = await Provider().ReadNotificationAsync(Event(wompiStatus), CancellationToken.None);

        // Assert
        Assert.Equal(expected, notification!.Outcome.Status);
    }

    [Fact]
    public async Task ReadNotificationAsync_Approved_ReportsTheAmountInMinorUnits()
    {
        // Arrange
        UseDecimalDigits(0);

        // Act
        var notification = await Provider().ReadNotificationAsync(Event("APPROVED"), CancellationToken.None);

        // Assert
        Assert.Equal((PaymentId.ToString(), 119_000L, "COP", "CARD", "1234-1610641025-49201"), (notification!.Reference, notification.Outcome.Amount!.Amount, notification.Outcome.Amount.Currency, notification.Outcome.PaymentMethodType, notification.Outcome.TransactionId));
    }

    [Fact]
    public async Task ReadNotificationAsync_PayoutEvent_IsNotACollectionEvent()
    {
        // Act
        var notification = await Provider().ReadNotificationAsync(Event("APPROVED").Replace("transaction.updated", "payout.updated", StringComparison.Ordinal), CancellationToken.None);

        // Assert
        Assert.Null(notification);
    }

    [Fact]
    public async Task ReadRefundNotificationAsync_RefundDeclined_NormalizesItWithItsReason()
    {
        // Arrange: the example of docs/wompi/reembolsos.md.
        const string body = "{\"event\":\"refund.updated\",\"data\":{\"refund\":{\"refund_id\":\"COa7Bk9m1\",\"transaction_id\":\"1292-1602113476-10985\",\"status\":\"DECLINED\",\"amount_in_cents\":50000,\"currency\":\"COP\",\"reference\":\"ORDER-2024-001\",\"code\":\"REFUND_DECLINED\",\"message\":\"El procesador rechazó el reembolso\"}},\"environment\":\"prod\",\"timestamp\":1530291411}";

        // Act
        var refund = await Provider().ReadRefundNotificationAsync(body, CancellationToken.None);

        // Assert
        Assert.Equal(("COa7Bk9m1", "1292-1602113476-10985", RefundStatus.Declined, 50_000L, "REFUND_DECLINED"), (refund!.RefundId, refund.TransactionId, refund.Status, refund.Amount!.Amount, refund.Code));
    }

    [Fact]
    public async Task ReadRefundNotificationAsync_TransactionEvent_IsNotARefund()
    {
        // Act
        var refund = await Provider().ReadRefundNotificationAsync(Event("APPROVED"), CancellationToken.None);

        // Assert
        Assert.Null(refund);
    }

    [Fact]
    public async Task GetOutcomeAsync_DeclinedThenApproved_TheApprovedWins()
    {
        // Arrange: a payer declined on the page can try again with the same reference.
        client.Setup(x => x.GetTransactionsByReferenceAsync("prv_test_tenant", PaymentId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Transaction("tx-2", "DECLINED", "2026-10-28T19:50:00.000Z"), Transaction("tx-1", "APPROVED", "2026-10-28T19:46:00.000Z")]);

        // Act
        var outcome = await Provider().GetOutcomeAsync(Payment(), credentials, CancellationToken.None);

        // Assert
        Assert.Equal(("tx-1", PaymentStatus.Succeeded), (outcome!.TransactionId, outcome.Status));
    }

    [Fact]
    public async Task GetOutcomeAsync_NoTransactionYet_IsNull()
    {
        // Arrange
        client.Setup(x => x.GetTransactionsByReferenceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        // Act
        var outcome = await Provider().GetOutcomeAsync(Payment(), credentials, CancellationToken.None);

        // Assert
        Assert.Null(outcome);
    }

    [Fact]
    public async Task ValidateAsync_KeysOfDifferentEnvironments_AreRejectedWithoutCallingWompi()
    {
        // Act
        var validation = await Provider().ValidateAsync("pub_test_tenant", new MerchantSecrets("prv_prod_tenant", "e", "i", null, null), CancellationToken.None);

        // Assert
        Assert.False(validation.IsValid);
        client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidateAsync_UnknownPublicKey_IsRejected()
    {
        // Arrange
        client.Setup(x => x.GetMerchantAsync("pub_test_tenant", It.IsAny<CancellationToken>())).ReturnsAsync((WompiMerchant?)null);

        // Act
        var validation = await Provider().ValidateAsync("pub_test_tenant", credentials.Secrets, CancellationToken.None);

        // Assert
        Assert.False(validation.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_PrivateKeyNotAccepted_IsRejected()
    {
        // Arrange
        client.Setup(x => x.GetMerchantAsync("pub_test_tenant", It.IsAny<CancellationToken>())).ReturnsAsync(new WompiMerchant { Name = "Comercio", Active = true });
        client.Setup(x => x.IsPrivateKeyAcceptedAsync("prv_test_tenant", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var validation = await Provider().ValidateAsync("pub_test_tenant", credentials.Secrets, CancellationToken.None);

        // Assert
        Assert.False(validation.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_KeysAccepted_ReportsEnvironmentAndMerchant()
    {
        // Arrange
        client.Setup(x => x.GetMerchantAsync("pub_test_tenant", It.IsAny<CancellationToken>())).ReturnsAsync(new WompiMerchant { Name = "Comercio", LegalName = "Comercio SAS", Active = true });
        client.Setup(x => x.IsPrivateKeyAcceptedAsync("prv_test_tenant", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var validation = await Provider().ValidateAsync("pub_test_tenant", credentials.Secrets, CancellationToken.None);

        // Assert
        Assert.Equal((true, MerchantAccountEnvironment.Sandbox, "Comercio SAS"), (validation.IsValid, validation.Environment, validation.MerchantName));
    }

    private WompiProvider Provider()
    {
        var options = Options.Create(new WompiOptions
        {
            CheckoutUrl = new Uri("https://checkout.wompi.co/p/"),
            LegalIdTypes = ["CC", "NIT"]
        });

        return new WompiProvider(client.Object, currencies.Object, options, Mock.Of<ILogger<WompiProvider>>());
    }

    private void UseDecimalDigits(short digits)
    {
        currencies.Setup(x => x.GetCurrencyAsync(It.IsAny<GetCurrencyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Currency.Create(Guid.NewGuid(), "Peso colombiano", "COP", "$", digits, 170));
    }

    private static PaymentAggregate Payment(long subTotal = 10_000_000L, long tax = 1_900_000L)
    {
        var buyer = Buyer.CreateWithoutShipping(Guid.NewGuid(), "Ana Restrepo", "3001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        return PaymentAggregate.Create(PaymentId, "Invoicing", Guid.NewGuid(), Money.FromLong(subTotal, "COP"), Money.FromLong(tax, "COP"), Money.FromLong(subTotal + tax, "COP"), buyer, "Cuota de administración", "Wompi", false, Guid.NewGuid(), Guid.NewGuid());
    }

    private static WompiTransaction Transaction(string id, string status, string createdAt) => new() { Id = id, Status = status, CreatedAt = createdAt, AmountInCents = 11_900_000L, Currency = "COP", Reference = PaymentId.ToString(), PaymentMethodType = "PSE" };

    private static string Event(string status) => $$"""
        {
          "event": "transaction.updated",
          "data": {
            "transaction": {
              "id": "1234-1610641025-49201",
              "amount_in_cents": 11900000,
              "reference": "{{PaymentId}}",
              "currency": "COP",
              "payment_method_type": "CARD",
              "status": "{{status}}"
            }
          },
          "environment": "test",
          "signature": { "properties": ["transaction.id", "transaction.status", "transaction.amount_in_cents"], "checksum": "x" },
          "timestamp": 1530291411
        }
        """;
}
