using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Test;

public class Temp
{
    [Fact]
    public void Test1()
    {
        var accountId = "512321";
        var merchantId = "508029";
        var apiLogin = "pRRXKOl8ikMmt9u";
        var apiKey = "4Vj8eK4rloUd272L48hsrarnUA";
        var publicKey = "PKaC6H4cEDJD919n705L544kSU";

        var currency = "COP";
        var amount = 1000;
        var referencia = "1234567890";

        var input = $"{apiKey}~{merchantId}~{referencia}~{amount}~{currency}";

        var result = GenerateMd5Hash(input);

        var expected = "e1252208b782e179cde40842cfb7520c";

        Assert.Equal(expected, result);

    }

    private static string GenerateMd5Hash(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
        return Convert.ToHexStringLower(hashBytes);
    }
}
