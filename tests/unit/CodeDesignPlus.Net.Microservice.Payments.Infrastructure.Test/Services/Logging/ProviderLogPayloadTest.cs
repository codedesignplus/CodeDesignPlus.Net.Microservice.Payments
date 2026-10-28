using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Logging;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi.Models;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Logging;

/// <summary>
/// A payout sent to the provider is logged without the beneficiary's account number or document (pendings/322).
/// </summary>
public class ProviderLogPayloadTest
{
    private const string AccountNumberProbe = "account-number-probe-660";
    private const string LegalIdProbe = "legal-id-probe-660";

    [Fact]
    public void Serialize_PayoutTransaction_OmitsAccountNumberAndDocument()
    {
        // Arrange
        var transaction = Transaction();

        // Act
        var json = ProviderLogPayload.Serialize(transaction);

        // Assert
        Assert.False(json.Contains(AccountNumberProbe, StringComparison.Ordinal) || json.Contains(LegalIdProbe, StringComparison.Ordinal), json);
    }

    [Fact]
    public void Serialize_PayoutTransaction_KeepsWhatIdentifiesThePayment()
    {
        // Arrange
        var transaction = Transaction();

        // Act
        var json = ProviderLogPayload.Serialize(transaction);

        // Assert
        Assert.Contains("\"reference\":\"payout-1\"", json, StringComparison.Ordinal);
    }

    private static WompiPayoutTransaction Transaction() => new()
    {
        LegalIdType = "NIT",
        LegalId = LegalIdProbe,
        BankId = "1007",
        AccountType = "AHORROS",
        AccountNumber = AccountNumberProbe,
        Name = "Proveedor SAS",
        Email = "proveedor@example.com",
        Amount = 11_900_000L,
        Reference = "payout-1"
    };
}
