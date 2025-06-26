using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Constants;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;
using CodeDesignPlus.Net.Security.Abstractions;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

public class Payu(IHttpClientFactory httpClientFactory, IOptions<PayuOptions> options, IUserContext user, ILogger<Payu> logger) : IPayu
{
    private readonly Newtonsoft.Json.JsonSerializerSettings settings = new()
    {
        ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy
            {
                OverrideSpecifiedNames = false
            },
        },
        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
        
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

        return await Request<BankRequest, BankResponse>(request, cancellationToken);
    }
    
    
    public async Task<TransactionResponse?> GetTransactioById(string id, CancellationToken cancellationToken)
    {
        var request = new TransactionRequest
        {
            Language = payuOptions.Language,
            Test = options.Value.IsTest,
            Merchant = new PayuMerchant
            {
                ApiLogin = payuOptions.ApiLogin,
                ApiKey = payuOptions.ApiKey
            },
            Details = new TransactionDetail
            {
                TransactionId = id
            }
        };

        return await Request<TransactionRequest, TransactionResponse>(request, cancellationToken);
    }


    public async Task ProcessPayment(Guid id, Domain.ValueObjects.Transaction transaction, Provider provider, CancellationToken cancellationToken)
    {
        var referenceCode = id.ToString();

        var value = $"{payuOptions.ApiKey}~{payuOptions.MerchantId}~{referenceCode}~{transaction.Order.Amount.Value}~{payuOptions.Currency}";

        var signature = GenerateMd5Hash(value);

        logger.LogWarning("Signature generated for Payu: {Signature} - {Value}", signature, value);

        var payuRequest = new PaymentRequest()
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
                        MerchantBuyerId = user.IdUser.ToString(),
                        FullName = transaction.Order.Buyer.FullName,
                        EmailAddress = transaction.Order.Buyer.EmailAddress,
                        ContactPhone = transaction.Order.Buyer.ContactPhone,
                        DniNumber = transaction.Order.Buyer.DniNumber,
                        DniType = transaction.Order.Buyer.DniType,
                    },
                    AdditionalValues = new PayuAdditionalValues
                    {
                        Value = new PayuAmount
                        {
                            Value = transaction.Order.Amount.Value,
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

                PaymentMethod = transaction.PaymentMethod,
                Payer = new PayuPayer
                {
                    MerchantPayerId = user.IdUser.ToString(),
                    FullName = transaction.Payer.FullName,
                    EmailAddress = transaction.Payer.EmailAddress,
                    ContactPhone = transaction.Payer.ContactPhone,
                    DniNumber = transaction.Payer.DniNumber,
                    DniType = transaction.Payer.DniType,
                },
                Type = payuOptions.TransactionType,
                PaymentCountry = payuOptions.PaymentCountry,
                DeviceSessionId = transaction.DeviceSessionId,
                Cookie = transaction.Cookie,
                IpAddress = transaction.IpAddress,
                UserAgent = transaction.UserAgent,
            }
        };

        if (transaction.Order.Buyer.ShippingAddress != null)
        {
            payuRequest.Transaction.Order.Buyer.ShippingAddress = new PayuAddress
            {
                Street1 = transaction.Order.Buyer.ShippingAddress.Street,
                City = transaction.Order.Buyer.ShippingAddress.City,
                State = transaction.Order.Buyer.ShippingAddress.State,
                Country = transaction.Order.Buyer.ShippingAddress.Country,
                PostalCode = transaction.Order.Buyer.ShippingAddress.PostalCode,
                Phone = transaction.Order.Buyer.ShippingAddress.Phone
            };
        }

        if (transaction.Payer.BillingAddress != null)
        {
            payuRequest.Transaction.Payer.BillingAddress = new PayuAddress
            {
                Street1 = transaction.Payer.BillingAddress.Street,
                City = transaction.Payer.BillingAddress.City,
                State = transaction.Payer.BillingAddress.State,
                Country = transaction.Payer.BillingAddress.Country,
                PostalCode = transaction.Payer.BillingAddress.PostalCode,
                Phone = transaction.Payer.BillingAddress.Phone
            };
        }

        if (transaction.CreditCard != null)
        {
            InfrastructureGuard.IsNull(transaction.CreditCard!, Errors.CreditCardCannotBeNull);

            payuRequest.Transaction.CreditCard = new PayuCreditCard
            {
                Number = transaction.CreditCard!.Number,
                ExpirationDate = transaction.CreditCard.ExpirationDate,
                SecurityCode = transaction.CreditCard.SecurityCode,
                Name = transaction.CreditCard.Name,
            };

            payuRequest.Transaction.ExtraParameters.Add("INSTALLMENTS_NUMBER", "1");

            // Si tienes cuotas para TC (transaction.InstallmentsNumber)
            // if (transaction.InstallmentsNumber.HasValue && transaction.InstallmentsNumber.Value > 0)
            // {
            //     payuRequest.Transaction.ExtraParameters.Add("INSTALLMENTS_NUMBER", transaction.InstallmentsNumber.Value.ToString());
            // }
        }

        if (transaction.Pse != null)
        {
            InfrastructureGuard.IsNull(transaction.Pse!, Errors.PseCannotBeNull);

            payuRequest.Transaction.ExtraParameters.Add("RESPONSE_URL", transaction.Pse!.PseResponseUrl);
            payuRequest.Transaction.ExtraParameters.Add("FINANCIAL_INSTITUTION_CODE", transaction.Pse.PseCode);
            payuRequest.Transaction.ExtraParameters.Add("USER_TYPE", transaction.Pse.TypePerson); // "N" o "J"
            payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE1", transaction.IpAddress); // Referencia opcional
            payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE2", transaction.Payer.DniType); // Referencia opcional
            payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE3", transaction.Payer.DniNumber); // Referencia opcional

            // Referencias opcionales de PSE si las usas
            // if (!string.IsNullOrEmpty(transaction.PseReference1))
            //    payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE1", transaction.PseReference1);
        }
        
        await Request<PaymentRequest, PayuResponse>(payuRequest, cancellationToken);
    }

    
    private async Task<TResponse> Request<T, TResponse>(T request, CancellationToken cancellationToken)
    {
        var json = CodeDesignPlus.Net.Serializers.JsonSerializer.Serialize(request, settings);

        logger.LogWarning("Sending request to Payu: {@Request}", json);

        var httpRequest = new HttpRequestMessage
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            Method = HttpMethod.Post
        };
        httpRequest.Headers.Add("Accept", "application/json");

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        return CodeDesignPlus.Net.Serializers.JsonSerializer.Deserialize<TResponse>(responseContent, settings);
    }

    
    private static string GenerateMd5Hash(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
        return Convert.ToHexStringLower(hashBytes);
    }
}

