using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Wompi;

/// <summary>
/// The signatures are computed exactly the way Wompi does. The expected values come from the reference client in
/// <c>tools/wompi</c>, whose signatures Wompi accepted in the sandbox on 28-oct-2026.
/// </summary>
public class WompiKeysTest
{
    private const string EventsSecret = "test_events_secret";

    private const string Event = """
        {
          "event": "transaction.updated",
          "data": {
            "transaction": {
              "id": "1234-1610641025-49201",
              "amount_in_cents": 11900000,
              "reference": "aa000000-0000-4000-8000-000000000001",
              "currency": "COP",
              "payment_method_type": "CARD",
              "status": "APPROVED",
              "created_at": "2026-10-28T19:46:59.213Z"
            }
          },
          "environment": "test",
          "signature": {
            "properties": ["transaction.id", "transaction.status", "transaction.amount_in_cents"],
            "checksum": "840DFBD129F49178B347F8370DAF01D71F5CBD72F331528FA81ADB66E42FD4A5"
          },
          "timestamp": 1530291411,
          "sent_at": "2026-10-28T19:47:01.000Z"
        }
        """;

    [Fact]
    public void IntegritySignature_WithExpiration_MatchesWompi()
    {
        // Act
        var signature = WompiKeys.IntegritySignature("aa000000-0000-4000-8000-000000000001", 11_900_000L, "COP", "2026-10-28T20:00:00.000Z", "test_integrity_secret");

        // Assert
        Assert.Equal("67abaec5fade67dec9c52ae2d9b7f2741f86640d70366aabb274f6ace38219be", signature);
    }

    [Fact]
    public void IntegritySignature_WithoutExpiration_MatchesWompi()
    {
        // Act
        var signature = WompiKeys.IntegritySignature("aa000000-0000-4000-8000-000000000001", 11_900_000L, "COP", null, "test_integrity_secret");

        // Assert
        Assert.Equal("4a647a04340f9c8c1eecfb54d077fd714c1fdb52b4df4af774b62747354c3363", signature);
    }

    [Fact]
    public void IsAuthenticEvent_SignedWithTheSecret_IsAccepted()
    {
        // Act
        var authentic = WompiKeys.IsAuthenticEvent(Event, EventsSecret);

        // Assert
        Assert.True(authentic);
    }

    [Fact]
    public void IsAuthenticEvent_OtherMerchantSecret_IsRejected()
    {
        // Act
        var authentic = WompiKeys.IsAuthenticEvent(Event, "another_merchant_events_secret");

        // Assert
        Assert.False(authentic);
    }

    [Fact]
    public void IsAuthenticEvent_TamperedStatus_IsRejected()
    {
        // Arrange
        var tampered = Event.Replace("\"status\": \"APPROVED\"", "\"status\": \"DECLINED\"", StringComparison.Ordinal);

        // Act
        var authentic = WompiKeys.IsAuthenticEvent(tampered, EventsSecret);

        // Assert
        Assert.False(authentic);
    }

    [Fact]
    public void IsAuthenticEvent_TamperedAmount_IsRejected()
    {
        // Arrange
        var tampered = Event.Replace("11900000", "100", StringComparison.Ordinal);

        // Act
        var authentic = WompiKeys.IsAuthenticEvent(tampered, EventsSecret);

        // Assert
        Assert.False(authentic);
    }

    [Fact]
    public void IsAuthenticEvent_SignedDateProperty_IsReadAsWompiWroteIt()
    {
        // Arrange: a date read as a DateTime would print differently from the text Wompi signed.
        var properties = "[\"transaction.id\", \"transaction.created_at\"]";
        var checksum = Sha256("1234-1610641025-494012026-10-28T19:46:59.213Z1530291411" + EventsSecret);
        var body = Event
            .Replace("[\"transaction.id\", \"transaction.status\", \"transaction.amount_in_cents\"]", properties, StringComparison.Ordinal)
            .Replace("\"id\": \"1234-1610641025-49201\"", "\"id\": \"1234-1610641025-49401\"", StringComparison.Ordinal)
            .Replace("840DFBD129F49178B347F8370DAF01D71F5CBD72F331528FA81ADB66E42FD4A5", checksum, StringComparison.Ordinal);

        // Act
        var authentic = WompiKeys.IsAuthenticEvent(body, EventsSecret);

        // Assert
        Assert.True(authentic);
    }

    [Fact]
    public void IsAuthenticEvent_NotJson_IsRejected()
    {
        // Act
        var authentic = WompiKeys.IsAuthenticEvent("merchant_id=1&sign=abc", EventsSecret);

        // Assert
        Assert.False(authentic);
    }

    [Theory]
    [InlineData("pub_test_abc", MerchantAccountEnvironment.Sandbox)]
    [InlineData("prv_test_abc", MerchantAccountEnvironment.Sandbox)]
    [InlineData("pub_prod_abc", MerchantAccountEnvironment.Production)]
    [InlineData("prv_prod_abc", MerchantAccountEnvironment.Production)]
    public void EnvironmentOf_WompiKey_FollowsItsPrefix(string key, MerchantAccountEnvironment expected)
    {
        // Act
        var environment = WompiKeys.EnvironmentOf(key);

        // Assert
        Assert.Equal(expected, environment);
    }

    [Fact]
    public void EnvironmentOf_KeyOfAnotherProvider_IsUnknown()
    {
        // Act
        var environment = WompiKeys.EnvironmentOf("sk_live_abc");

        // Assert
        Assert.Null(environment);
    }

    private static string Sha256(string value) => Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));
}
