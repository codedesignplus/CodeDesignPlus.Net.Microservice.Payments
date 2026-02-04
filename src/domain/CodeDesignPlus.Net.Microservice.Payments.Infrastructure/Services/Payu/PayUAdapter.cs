using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;
using CodeDesignPlus.Net.Security.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

public class PayUAdapter(IHttpClientFactory httpClientFactory, IOptions<PayuOptions> options, IUserContext user, ILogger<PayUAdapter> logger, IHttpContextAccessor httpContextAccessor)
    : IPayu
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

    public PaymentProvider Provider => PaymentProvider.Payu;

    public async Task<BankResponse?> GetBanksListAsync(CancellationToken cancellationToken)
    {
        var request = new BankRequest
        {
            Language = payuOptions.Language,
            Command = "GET_BANKS_LIST",
            Test = options.Value.IsTest,
            Merchant = new Merchant
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

        return await Request<BankRequest, BankResponse>(request, "payments-api/4.0/service.cgi", cancellationToken);
    }

    public async Task<InitiatePaymentResponseDto> InitiatePaymentAsync(PaymentAggregate payment, CancellationToken cancellationToken)
    {
        var payuRequest = new PayuPaymentRequest()
        {
            Language = payuOptions.Language,
            Test = payuOptions.IsTest,
            Merchant = new Merchant
            {
                ApiKey = payuOptions.ApiKey,
                ApiLogin = payuOptions.ApiLogin
            },
            Transaction = new PayuTransaction
            {
                Order = new PayuOrder
                {
                    AccountId = payuOptions.AccountId,
                    ReferenceCode = payment.Id.ToString(),
                    Description = payment.Description,
                    Language = payuOptions.Language,
                    NotifyUrl = $"{payuOptions.NotificationUrl}/{PaymentProvider.Payu}",
                    Buyer = new PayuBuyer
                    {
                        MerchantBuyerId = user.IdUser.ToString(),
                        FullName = payment.Payer.FullName,
                        EmailAddress = payment.Payer.EmailAddress,
                        ContactPhone = payment.Payer.ContactPhone,
                        DniNumber = payment.Payer.DniNumber,
                        DniType = payment.Payer.DniType,
                    },
                    AdditionalValues = new PayuAdditionalValues
                    {
                        SubTotal = new PayuAmount
                        {
                            Value = payment.SubTotal.Value,
                            Currency = payment.SubTotal.Currency ?? payuOptions.Currency
                        },
                        Tax = new PayuAmount
                        {
                            Value = payment.Tax.Value,
                            Currency = payment.Tax.Currency ?? payuOptions.Currency
                        },
                        Total = new PayuAmount
                        {
                            Value = payment.Total.Value,
                            Currency = payment.Total.Currency ?? payuOptions.Currency
                        }
                    },
                },

                PaymentMethod = payment.PaymentMethod.Type,
                Payer = new PayuPayer
                {
                    MerchantPayerId = user.IdUser.ToString(),
                    FullName = payment.Payer.FullName,
                    EmailAddress = payment.Payer.EmailAddress,
                    ContactPhone = payment.Payer.ContactPhone,
                    DniNumber = payment.Payer.DniNumber,
                    DniType = payment.Payer.DniType,
                },
                Type = payuOptions.TransactionType,
                PaymentCountry = payuOptions.PaymentCountry,
                DeviceSessionId = user.IdUser.ToString(),
                Cookie = httpContextAccessor.HttpContext?.Request.Cookies["PaymentCookie"] ?? Guid.NewGuid().ToString(),
                IpAddress = user.IpAddress,
                UserAgent = user.UserAgent,
            }
        };

        if (payment.PaymentMethod.CreditCard != null)
        {
            payuRequest.Transaction.CreditCard = new PayuCreditCard
            {
                Number = payment.PaymentMethod.CreditCard!.Number,
                ExpirationDate = payment.PaymentMethod.CreditCard.ExpirationDate,
                SecurityCode = payment.PaymentMethod.CreditCard.SecurityCode,
                Name = payment.PaymentMethod.CreditCard.Name,
            };

            payuRequest.Transaction.ExtraParameters.Add("INSTALLMENTS_NUMBER", payment.PaymentMethod.CreditCard.InstallmentsNumber.ToString());
        }

        if (payment.PaymentMethod.Pse != null)
        {
            payuRequest.Transaction.ExtraParameters.Add("RESPONSE_URL", payment.PaymentMethod.Pse!.PseResponseUrl);
            payuRequest.Transaction.ExtraParameters.Add("FINANCIAL_INSTITUTION_CODE", payment.PaymentMethod.Pse.PseCode);
            payuRequest.Transaction.ExtraParameters.Add("USER_TYPE", payment.PaymentMethod.Pse.TypePerson);
            payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE1", user.IpAddress);
            payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE2", $"Reference Code: {payment.Id}");
            payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE3", payment.Module);
        }

        var providerResponse = await ProcessPaymentAsync(payuRequest, cancellationToken);

        var responseDto = new InitiatePaymentResponseDto
        {
            Success = providerResponse.TransactionResponse.State == "APPROVED" || providerResponse.TransactionResponse.State == "PENDING",
            PaymentId = payment.Id,
            ProviderTransactionId = providerResponse.TransactionResponse.TransactionId,
            ProviderResponse = new Dictionary<string, string> { { "raw", JsonSerializer.Serialize(providerResponse) } }
        };

        if (payment.PaymentMethod.Pse != null && !string.IsNullOrEmpty(providerResponse.TransactionResponse.ExtraParameters?.GetValueOrDefault("BANK_URL")))
        {
            responseDto.NextAction = NextActionType.Redirect;
            responseDto.RedirectUrl = providerResponse.TransactionResponse.ExtraParameters.GetValueOrDefault("BANK_URL");
        }
        else
        {
            responseDto.NextAction = NextActionType.WaitConfirmation;
        }

        return responseDto;
    }

    public async Task<ProcessWebhookResponseDto> ProcessWebhookAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var response = new ProcessWebhookResponseDto();

        if (request.Form.ContainsKey("merchant_id"))
        {
            response.RawData = request.Form.Keys.ToDictionary(k => k, k => request.Form[k].ToString());

            var merchantId = response.RawData.GetValueOrDefault("merchant_id");
            var currency = response.RawData.GetValueOrDefault("currency");
            var state = response.RawData.GetValueOrDefault("state_pol");
            var value = response.RawData.GetValueOrDefault("value");
            var receivedSignature = response.RawData.GetValueOrDefault("sign");
            var referenceSale = response.RawData.GetValueOrDefault("reference_sale");

            InfrastructureGuard.IsNullOrEmpty(merchantId, Errors.MerchantIdMissing);
            InfrastructureGuard.IsNullOrEmpty(currency, Errors.CurrencyMissing);
            InfrastructureGuard.IsNullOrEmpty(state, Errors.StateMissing);
            InfrastructureGuard.IsNullOrEmpty(value, Errors.ValueMissing);
            InfrastructureGuard.IsNullOrEmpty(receivedSignature, Errors.SignatureMissing);
            InfrastructureGuard.IsNullOrEmpty(referenceSale, Errors.ReferenceSaleMissing);

            InfrastructureGuard.IsTrue(!Guid.TryParse(referenceSale, out var paymentId), Errors.InvalidReferenceSale);

            response.PaymentId = paymentId;

            var valorDecimal = decimal.Parse(value!, System.Globalization.CultureInfo.InvariantCulture);
            var valueString = valorDecimal.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            var expectedSignature = GenerateMd5Hash($"{payuOptions.ApiKey}~{merchantId}~{referenceSale}~{valueString}~{currency}~{state}");

            response.IsSignatureValid = string.Equals(expectedSignature, receivedSignature, StringComparison.OrdinalIgnoreCase);

            response.FinalStatus = state switch
            {
                "4" => PaymentStatus.Succeeded,
                "6" => PaymentStatus.Failed,
                "5" => PaymentStatus.Expired,
                _ => PaymentStatus.Unknown
            };
        }
        else
        {
            response.IsSignatureValid = false;
        }

        return await Task.FromResult(response);
    }

    private async Task<PayuPaymentResponse> ProcessPaymentAsync(PayuPaymentRequest request, CancellationToken cancellationToken)
    {
        var value = $"{payuOptions.ApiKey}~{payuOptions.MerchantId}~{request.Transaction.Order.ReferenceCode}~{request.Transaction.Order.AdditionalValues.Total.Value}~{payuOptions.Currency}";

        var signature = GenerateMd5Hash(value);

        request.Transaction.Order.Signature = signature;

        return await Request<PayuPaymentRequest, PayuPaymentResponse>(request, "payments-api/4.0/service.cgi", cancellationToken);
    }

    private async Task<TResponse> Request<T, TResponse>(T request, string url, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, settings);

        logger.LogWarning("Sending request to Payu: {@Request}", json);

        var httpRequest = new HttpRequestMessage
        {
            RequestUri = new Uri(url, UriKind.Relative),
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            Method = HttpMethod.Post
        };

        httpRequest.Headers.Add("Accept", "application/json");

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        logger.LogWarning("Received response from Payu: {@Response}", responseContent);

        return JsonSerializer.Deserialize<TResponse>(responseContent, settings);
    }

    private static string GenerateMd5Hash(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
        return Convert.ToHexStringLower(hashBytes);
    }

    private static PaymentStatus ToPaymentStatus(string state)
    {
        return state switch
        {
            "APPROVED" => PaymentStatus.Succeeded,
            "PENDING" => PaymentStatus.Pending,
            "DECLINED" => PaymentStatus.Failed,
            _ => PaymentStatus.Unknown
        };
    }

}
