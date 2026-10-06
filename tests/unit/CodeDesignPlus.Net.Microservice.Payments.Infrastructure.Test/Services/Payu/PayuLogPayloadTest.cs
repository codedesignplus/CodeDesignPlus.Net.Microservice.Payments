using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Logging;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Payu;

/// <summary>
/// La petición a PayU se escribe en el log sin el CVV, sin el número de la tarjeta y sin la clave del comercio
/// (pendings/322).
/// </summary>
public class PayuLogPayloadTest
{
    private const string SecurityCodeProbe = "cvv-probe-322";
    private const string CardNumberProbe = "4111-pan-probe-322";
    private const string ApiKeyProbe = "api-key-probe-322";

    [Fact]
    public void Serialize_CardPayment_OmitsSecurityCode()
    {
        // Arrange
        var request = new PayuTransaction { CreditCard = new PayuCreditCard { SecurityCode = SecurityCodeProbe, ExpirationDate = "2030/12" } };

        // Act
        var json = PayuLogPayload.Serialize(request);

        // Assert
        Assert.DoesNotContain(SecurityCodeProbe, json, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_CardPayment_KeepsTheRestOfTheCard()
    {
        // Arrange
        var request = new PayuTransaction { CreditCard = new PayuCreditCard { SecurityCode = SecurityCodeProbe, ExpirationDate = "2030/12" } };

        // Act
        var json = PayuLogPayload.Serialize(request);

        // Assert
        Assert.Contains("\"expirationDate\":\"2030/12\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_Tokenization_OmitsCardNumber()
    {
        // Arrange
        var request = new CreditCardTokenPayload { Number = CardNumberProbe, ExpirationDate = "2030/12" };

        // Act
        var json = PayuLogPayload.Serialize(request);

        // Assert
        Assert.DoesNotContain(CardNumberProbe, json, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_Merchant_OmitsApiKey()
    {
        // Arrange
        var merchant = new Merchant { ApiLogin = "login", ApiKey = ApiKeyProbe };

        // Act
        var json = PayuLogPayload.Serialize(merchant);

        // Assert
        Assert.DoesNotContain(ApiKeyProbe, json, StringComparison.Ordinal);
    }
}
