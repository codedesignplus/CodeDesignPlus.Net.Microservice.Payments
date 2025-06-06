using System.Net.Http.Json;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

public class Payu(IHttpClientFactory httpClientFactory, IOptions<PayuOptions> options) : IPayu
{
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

        var resposne = await this.httpClient.PostAsJsonAsync(options.Value.BaseUrl, request, cancellationToken);

        return await resposne.Content.ReadFromJsonAsync<BankResponse>(cancellationToken);
    }

    public Task<Domain.Models.TransactionResponse> ProcessPayment(Guid id, Domain.ValueObjects.Transaction transaction, Provider provider, CancellationToken cancellationToken)
    {
        return ProcessPayment(id, transaction, provider, [], cancellationToken);
    }

    public async Task<Domain.Models.TransactionResponse> ProcessPayment(Guid id, Domain.ValueObjects.Transaction transaction, Provider provider, Dictionary<string, string> extraParametrs, CancellationToken cancellationToken)
    {
        var referenceCode = id.ToString();

        var payuRequest = new PayuRequest()
        {
            Language = payuOptions.Language,
            IsTest = payuOptions.IsTest,
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
                    Signature = $"{payuOptions.ApiKey}~{payuOptions.MerchantId}~{referenceCode}~{payuOptions.Currency}",
                    Buyer = new PayuBuyer
                    {
                        FullName = transaction.Order.Buyer.FullName,
                        EmailAddress = transaction.Order.Buyer.EmailAddress,
                        ContactPhone = transaction.Order.Buyer.ContactPhone,
                        ShippingAddress = new PayuAddress
                        {
                            Street1 = transaction.Order.Buyer.ShippingAddress.Street,
                            City = transaction.Order.Buyer.ShippingAddress.City,
                            State = transaction.Order.Buyer.ShippingAddress.State,
                            Country = transaction.Order.Buyer.ShippingAddress.Country,
                            PostalCode = transaction.Order.Buyer.ShippingAddress.PostalCode
                        }
                    },
                    AdditionalValues = new PayuAdditionalValues
                    {
                        TX_VALUE = new PayuAmount
                        {
                            Value = transaction.Order.Ammount.Value,
                            Currency = payuOptions.Currency
                        },
                        TX_TAX = new PayuAmount
                        {
                            Value = transaction.Order.Tax.Value,
                            Currency = payuOptions.Currency
                        },
                        TX_TAX_RETURN_BASE = new PayuAmount
                        {
                            Value = transaction.Order.TaxReturnBase.Value,
                            Currency = payuOptions.Currency
                        }
                    },
                },
                Payer = new PayuPayer
                {
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

        var json = JsonSerializer.Serialize(payuRequest);

        var response = await httpClient.PostAsJsonAsync("/payments-api/4.0/service.cgi", json, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        return new Domain.Models.TransactionResponse
        {
            Id = id,
            Provider = "Payu",
            Request = json,
            Response = responseContent
        };
    }
}

