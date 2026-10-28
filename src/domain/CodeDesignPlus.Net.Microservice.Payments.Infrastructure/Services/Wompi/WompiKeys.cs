using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;

/// <summary>
/// What Wompi's keys and signatures say, computed exactly the way Wompi does.
/// </summary>
public static class WompiKeys
{
    // Wompi encodes the environment in the key itself (Desarrollo › Programadores): these prefixes are its API
    // contract, not a setting of this deployment.
    private static readonly string[] SandboxPrefixes = ["pub_test_", "prv_test_"];
    private static readonly string[] ProductionPrefixes = ["pub_prod_", "prv_prod_"];

    /// <summary>
    /// The environment a public or private key belongs to.
    /// </summary>
    /// <param name="key">A Wompi public or private key.</param>
    /// <returns>The environment, or null when the key is not a Wompi key.</returns>
    public static MerchantAccountEnvironment? EnvironmentOf(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (SandboxPrefixes.Any(prefix => key.StartsWith(prefix, StringComparison.Ordinal)))
        {
            return MerchantAccountEnvironment.Sandbox;
        }

        if (ProductionPrefixes.Any(prefix => key.StartsWith(prefix, StringComparison.Ordinal)))
        {
            return MerchantAccountEnvironment.Production;
        }

        return null;
    }

    /// <summary>
    /// The integrity signature of the hosted page: SHA-256 of the reference, the amount in cents, the currency, the
    /// expiration time (when there is one) and the integrity secret, concatenated.
    /// </summary>
    /// <param name="reference">The payment reference.</param>
    /// <param name="amountInCents">The amount in cents.</param>
    /// <param name="currency">The ISO 4217 currency.</param>
    /// <param name="expirationTime">The expiration time exactly as it goes in the URL, or null.</param>
    /// <param name="integritySecret">The merchant's integrity secret.</param>
    /// <returns>The signature in lowercase hexadecimal.</returns>
    public static string IntegritySignature(string reference, long amountInCents, string currency, string? expirationTime, string integritySecret)
    {
        var chain = string.Concat(reference, amountInCents.ToString(CultureInfo.InvariantCulture), currency, expirationTime ?? string.Empty, integritySecret);

        return Sha256(chain);
    }

    /// <summary>
    /// Checks the checksum of an event: SHA-256 of the values named in <c>signature.properties</c> (read from
    /// <c>data</c>), the <c>timestamp</c> and the events secret, concatenated.
    /// </summary>
    /// <param name="body">The event as it arrived.</param>
    /// <param name="eventsSecret">The events secret of the merchant the event claims to come from.</param>
    /// <returns>Whether the checksum matches.</returns>
    public static bool IsAuthenticEvent(string body, string eventsSecret)
    {
        var root = Parse(body);

        if (root is null)
        {
            return false;
        }

        var properties = root.SelectToken("signature.properties") as JArray;
        var checksum = root.SelectToken("signature.checksum")?.Value<string>();
        var timestamp = root["timestamp"];
        var data = root["data"];

        if (properties is null || string.IsNullOrEmpty(checksum) || timestamp is null || data is null)
        {
            return false;
        }

        var chain = new StringBuilder();

        foreach (var property in properties.Values<string>())
        {
            if (string.IsNullOrEmpty(property) || data.SelectToken(property) is not JValue value)
            {
                return false;
            }

            chain.Append(ValueOf(value));
        }

        chain.Append(ValueOf((JValue)timestamp));
        chain.Append(eventsSecret);

        var expected = Encoding.ASCII.GetBytes(Sha256(chain.ToString()));
        var received = Encoding.ASCII.GetBytes(checksum.ToLowerInvariant());

        return CryptographicOperations.FixedTimeEquals(expected, received);
    }

    /// <summary>
    /// Parses a body from Wompi keeping every value as it was written: a date stays the text Wompi signed, not a
    /// <see cref="DateTime"/> that prints differently.
    /// </summary>
    /// <param name="body">The body as it arrived.</param>
    /// <returns>The parsed object, or null when it is not a JSON object.</returns>
    public static JObject? Parse(string body)
    {
        try
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(body, new Newtonsoft.Json.JsonSerializerSettings
            {
                DateParseHandling = Newtonsoft.Json.DateParseHandling.None,
                FloatParseHandling = Newtonsoft.Json.FloatParseHandling.Decimal
            });
        }
        catch (Newtonsoft.Json.JsonException)
        {
            return null;
        }
    }

    private static string ValueOf(JValue value) => Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Sha256(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
