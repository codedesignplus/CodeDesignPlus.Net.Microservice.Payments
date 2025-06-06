using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;
using CodeDesignPlus.Net.Security.Abstractions;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

public class Payu(IHttpClientFactory httpClientFactory, IOptions<PayuOptions> options, IUserContext user) : IPayu
{
    private readonly Newtonsoft.Json.JsonSerializerSettings settings = new()
    {
        ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy
            {
                OverrideSpecifiedNames = false
            }
        },
        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
    };

    private readonly HttpClient httpClient = httpClientFactory.CreateClient("Payu");

    private readonly PayuOptions payuOptions = options.Value;

    public async Task<BankResponse?> GetBanksListAsync(CancellationToken cancellationToken)
    {
        var request = new BankRequest
        {
            Language = payuOptions.Language,
            Command = "GET_BANKS_LIST",
            Test = options.Value.IsTest,
            Merchant = new PayuMerchant
            {
                ApiLogin = payuOptions.ApiLogin,
                ApiKey = payuOptions.ApiKey
            },
            BankListInformation = new BankListInformation
            {
                PaymentMethod = "PSE",
                PaymentCountry = payuOptions.PaymentCountry,
            },
        };

        var resposne = await this.httpClient.PostAsJsonAsync(options.Value.Url, request, cancellationToken);

        return await resposne.Content.ReadFromJsonAsync<BankResponse>(cancellationToken);
    }

    public Task<Domain.Models.TransactionResponse> ProcessPayment(Guid id, Domain.ValueObjects.Transaction transaction, Provider provider, CancellationToken cancellationToken)
    {
        return ProcessPayment(id, transaction, provider, [], cancellationToken);
    }

    public async Task<Domain.Models.TransactionResponse> ProcessPayment(Guid id, Domain.ValueObjects.Transaction transaction, Provider provider, Dictionary<string, string> extraParametrs, CancellationToken cancellationToken)
    {
        var referenceCode = id.ToString();

        var signature = GenerarFirmaHMACSHA256($"{payuOptions.ApiKey}~{payuOptions.MerchantId}~{referenceCode}~{transaction.Order.Ammount.Value}~{payuOptions.Currency}", payuOptions.SecretKey);

        var payuRequest = new PayuRequest()
        {
            Language = payuOptions.Language,
            Test = payuOptions.IsTest,
            Merchant = new PayuMerchant
            {
                ApiKey = payuOptions.ApiKey,
                ApiLogin = payuOptions.ApiLogin
            },
            Transaction = new PayuTransaction
            {
                Order = new PayuOrder
                {
                    AccountId = payuOptions.AccountId,
                    ReferenceCode = referenceCode,
                    Description = transaction.Order.Description,
                    Language = payuOptions.Language,
                    Signature = signature,
                    Buyer = new PayuBuyer
                    {
                        //MerchantBuyerId = user.IdUser.ToString(),
                        FullName = transaction.Order.Buyer.FullName,
                        EmailAddress = transaction.Order.Buyer.EmailAddress,
                        ContactPhone = transaction.Order.Buyer.ContactPhone,
                        DniNumber = transaction.Order.Buyer.DniNumber,
                        ShippingAddress = new PayuAddress
                        {
                            Street1 = transaction.Order.Buyer.ShippingAddress.Street,
                            City = transaction.Order.Buyer.ShippingAddress.City,
                            State = transaction.Order.Buyer.ShippingAddress.State,
                            Country = transaction.Order.Buyer.ShippingAddress.Country,
                            PostalCode = transaction.Order.Buyer.ShippingAddress.PostalCode,
                            Phone = transaction.Order.Buyer.ShippingAddress.Phone
                        }
                    },
                    ShippingAddress = new PayuAddress
                    {
                        Street1 = transaction.Order.Buyer.ShippingAddress.Street,
                        City = transaction.Order.Buyer.ShippingAddress.City,
                        State = transaction.Order.Buyer.ShippingAddress.State,
                        Country = transaction.Order.Buyer.ShippingAddress.Country,
                        PostalCode = transaction.Order.Buyer.ShippingAddress.PostalCode,
                        Phone = transaction.Order.Buyer.ShippingAddress.Phone
                    },
                    AdditionalValues = new PayuAdditionalValues
                    {
                        Value = new PayuAmount
                        {
                            Value = transaction.Order.Ammount.Value,
                            Currency = payuOptions.Currency
                        },
                        Tax = new PayuAmount
                        {
                            Value = transaction.Order.Tax.Value,
                            Currency = payuOptions.Currency
                        },
                        TaxReturnBase = new PayuAmount
                        {
                            Value = transaction.Order.TaxReturnBase.Value,
                            Currency = payuOptions.Currency
                        }
                    },
                },
                Payer = new PayuPayer
                {
                    //MerchantPayerId = user.IdUser.ToString(),
                    FullName = transaction.Payer.FullName,
                    EmailAddress = transaction.Payer.EmailAddress,
                    ContactPhone = transaction.Payer.ContactPhone,
                    DniNumber = transaction.Payer.DniNumber,
                    BillingAddress = new PayuAddress
                    {
                        Street1 = transaction.Payer.BillingAddress.Street,
                        City = transaction.Payer.BillingAddress.City,
                        State = transaction.Payer.BillingAddress.State,
                        Country = transaction.Payer.BillingAddress.Country,
                        PostalCode = transaction.Payer.BillingAddress.PostalCode
                    }
                },
                CreditCard = new PayuCreditCard
                {
                    Number = transaction.CreditCard.Number,
                    ExpirationDate = transaction.CreditCard.ExpirationDate,
                    SecurityCode = transaction.CreditCard.SecurityCode,
                    Name = transaction.CreditCard.Name,
                },
                Type = payuOptions.TransactionType,
                PaymentMethod = transaction.PaymentMethod,
                PaymentCountry = payuOptions.PaymentCountry,
                DeviceSessionId = transaction.DeviceSessionId,
                Cookie = transaction.Cookie,
                IpAddress = transaction.IpAddress,
                UserAgent = transaction.UserAgent,
                ExtraParameters = extraParametrs,
            }
        };

        var json = CodeDesignPlus.Net.Serializers.JsonSerializer.Serialize(payuRequest, settings);

        var httpRequest = new HttpRequestMessage
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            Method = HttpMethod.Post
        };
        httpRequest.Headers.Add("Accept", "application/json");

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        return new Domain.Models.TransactionResponse
        {
            Id = id,
            Provider = "Payu",
            Request = json,
            Response = responseContent
        };
    }

    private string GenerarFirmaHMACSHA256(string v, object secretKey)
    {
        throw new NotImplementedException();
    }

    private static string GenerarFirmaMD5(string texto)
    {
        // Usamos 'using' para asegurarnos de que los recursos del objeto MD5 se liberen correctamente.
        using var md5 = MD5.Create();
        // El algoritmo MD5 trabaja con bytes, no con strings.
        // Por eso, primero convertimos la cadena a un arreglo de bytes usando codificación UTF-8.
        byte[] inputBytes = Encoding.UTF8.GetBytes(texto);

        // Ahora, calculamos el hash MD5. El resultado es otro arreglo de bytes (de 16 bytes de longitud).
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        // Para obtener la firma en el formato hexadecimal que necesitas (ej: "ba9ffa..."),
        // debemos convertir cada byte del hash a su representación de 2 caracteres hexadecimales.
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            // "x2" formatea el byte como una cadena hexadecimal de 2 caracteres en minúsculas.
            // Ejemplo: el byte '10' se convierte en "0a", el byte '255' se convierte en "ff".
            sb.Append(hashBytes[i].ToString("x2"));
        }

        // Devolvemos el string construido.
        return sb.ToString();
    }

     public static string GenerarFirmaHMACSHA256(string mensaje, string claveSecreta)
    {
        var keyBytes = Encoding.UTF8.GetBytes(claveSecreta);
        var messageBytes = Encoding.UTF8.GetBytes(mensaje);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            var hashBytes = hmac.ComputeHash(messageBytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
    
    
}

