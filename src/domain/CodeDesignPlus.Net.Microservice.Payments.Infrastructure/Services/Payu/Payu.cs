using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;
using CodeDesignPlus.Net.Security.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

public class Payu(IHttpClientFactory httpClientFactory, IOptions<PayuOptions> options, IUserContext user, ILogger<Payu> logger, IHttpContextAccessor httpContextAccessor) : IPayu
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

        return await Request<BankRequest, BankResponse>(request, cancellationToken);
    }

    public async Task<PaymentResponseDto> InitiatePaymentAsync(InitiatePaymentCommand command, CancellationToken cancellationToken)
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
                    ReferenceCode = command.Id.ToString(),
                    Description = command.Description,
                    Language = payuOptions.Language,
                    Buyer = new PayuBuyer
                    {
                        MerchantBuyerId = user.IdUser.ToString(),
                        FullName = command.Payer.FullName,
                        EmailAddress = command.Payer.EmailAddress,
                        ContactPhone = command.Payer.ContactPhone,
                        DniNumber = command.Payer.DniNumber,
                        DniType = command.Payer.DniType,
                    },
                    AdditionalValues = new PayuAdditionalValues
                    {
                        SubTotal = new PayuAmount
                        {
                            Value = command.SubTotal.Value,
                            Currency = command.SubTotal.Currency ?? payuOptions.Currency
                        },
                        Tax = new PayuAmount
                        {
                            Value = command.Tax.Value,
                            Currency = command.Tax.Currency ?? payuOptions.Currency
                        },
                        Total = new PayuAmount
                        {
                            Value = command.Total.Value,
                            Currency = command.Total.Currency ?? payuOptions.Currency
                        }
                    },
                },

                PaymentMethod = command.PaymentMethod.Type,
                Payer = new PayuPayer
                {
                    MerchantPayerId = user.IdUser.ToString(),
                    FullName = command.Payer.FullName,
                    EmailAddress = command.Payer.EmailAddress,
                    ContactPhone = command.Payer.ContactPhone,
                    DniNumber = command.Payer.DniNumber,
                    DniType = command.Payer.DniType,
                },
                Type = payuOptions.TransactionType,
                PaymentCountry = payuOptions.PaymentCountry,
                DeviceSessionId = user.IdUser.ToString(),
                Cookie = httpContextAccessor.HttpContext?.Request.Cookies["PaymentCookie"] ?? Guid.NewGuid().ToString(),
                IpAddress = user.IpAddress,
                UserAgent = user.UserAgent,
            }
        };

        if (command.PaymentMethod.CreditCard != null)
        {
            payuRequest.Transaction.CreditCard = new PayuCreditCard
            {
                Number = command.PaymentMethod.CreditCard!.Number,
                ExpirationDate = command.PaymentMethod.CreditCard.ExpirationDate,
                SecurityCode = command.PaymentMethod.CreditCard.SecurityCode,
                Name = command.PaymentMethod.CreditCard.Name,
            };

            payuRequest.Transaction.ExtraParameters.Add("INSTALLMENTS_NUMBER", command.PaymentMethod.CreditCard.InstallmentsNumber.ToString());
        }

        if (command.PaymentMethod.Pse != null)
        {
            payuRequest.Transaction.ExtraParameters.Add("RESPONSE_URL", command.PaymentMethod.Pse!.PseResponseUrl);
            payuRequest.Transaction.ExtraParameters.Add("FINANCIAL_INSTITUTION_CODE", command.PaymentMethod.Pse.PseCode);
            payuRequest.Transaction.ExtraParameters.Add("USER_TYPE", command.PaymentMethod.Pse.TypePerson);
            payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE1", user.IpAddress);
            payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE2", $"Reference Code: {command.Id}");
            payuRequest.Transaction.ExtraParameters.Add("PSE_REFERENCE3", command.Module);
        }

        var response = await ProcessPayment(payuRequest, cancellationToken);

        var bankResponse = new FinancialNetwork(
            PaymentNetworkResponseCode: response.TransactionResponse.PaymentNetworkResponseCode,
            PaymentNetworkResponseErrorMessage: response.TransactionResponse.PaymentNetworkResponseErrorMessage,
            TrazabilityCode: response.TransactionResponse.TrazabilityCode,
            AuthorizationCode: response.TransactionResponse.AuthorizationCode,
            ResponseCode: response.TransactionResponse.ResponseCode
        );

        return new PaymentResponseDto{
            Success = response.TransactionResponse.State == "APPROVED",
            Status = ToPaymentStatus(response.TransactionResponse.State),
            TransactionId = response.TransactionResponse.TransactionId,
            RedirectUrl = response.TransactionResponse.ExtraParameters.GetValueOrDefault("BANK_URL"),
            Message = response.TransactionResponse.ResponseMessage,
            FinancialNetwork = bankResponse
        };
    }

    public async Task<PaymentResponseDto> CheckStatusAsync(string id, CancellationToken cancellationToken)
    {
        var response = await this.GetByTransactionId(id, cancellationToken);

        logger.LogInformation("Checking status for transaction {TransactionId}: {@Response}", id, response);

        var bankResponse = new FinancialNetwork(
            PaymentNetworkResponseCode: response.Result.Payload.PaymentNetworkResponseCode,
            PaymentNetworkResponseErrorMessage: response.Result.Payload.PaymentNetworkResponseErrorMessage,
            TrazabilityCode: response.Result.Payload.TrazabilityCode,
            AuthorizationCode: response.Result.Payload.AuthorizationCode,
            ResponseCode: response.Result.Payload.ResponseCode
        );

        return new PaymentResponseDto{
            Success = response.Result.Payload.State == "APPROVED",
            Status = ToPaymentStatus(response.Result.Payload.State),
            TransactionId = id,
            RedirectUrl = response.Result.Payload.ExtraParameters.GetValueOrDefault("BANK_URL"),
            Message = response.Result.Payload.ResponseMessage,
            FinancialNetwork = bankResponse
        };
    }

    private async Task<PayuReferenceCodeResponse> GetByReferenceCode(Guid id, CancellationToken cancellationToken)
    {
        var request = new PayuReferenceCodeRequest
        {
            Language = payuOptions.Language,
            Command = "ORDER_DETAIL_BY_REFERENCE_CODE",
            Test = options.Value.IsTest,
            Merchant = new Merchant
            {
                ApiLogin = payuOptions.ApiLogin,
                ApiKey = payuOptions.ApiKey
            },
            Details = new()
            {
                ReferenceCode = id.ToString(),
            }
        };

        return await Request<PayuReferenceCodeRequest, PayuReferenceCodeResponse>(request, cancellationToken);
    }


    private async Task<PayuTransactionResponse> GetByTransactionId(string id, CancellationToken cancellationToken)
    {
        var request = new PayuTransactionRequest
        {
            Language = payuOptions.Language,
            Command = "TRANSACTION_RESPONSE_DETAIL",
            Test = options.Value.IsTest,
            Merchant = new Merchant
            {
                ApiLogin = payuOptions.ApiLogin,
                ApiKey = payuOptions.ApiKey
            },
            Details = new()
            {
                TransactionId = id
            }
        };

        return await Request<PayuTransactionRequest, PayuTransactionResponse>(request, cancellationToken);
    }

    private async Task<PayuPaymentResponse> ProcessPayment(PayuPaymentRequest request, CancellationToken cancellationToken)
    {
        var value = $"{payuOptions.ApiKey}~{payuOptions.MerchantId}~{request.Transaction.Order.ReferenceCode}~{request.Transaction.Order.AdditionalValues.Total.Value}~{payuOptions.Currency}";

        var signature = GenerateMd5Hash(value);

        request.Transaction.Order.Signature = signature;

        return await Request<PayuPaymentRequest, PayuPaymentResponse>(request, cancellationToken);
    }

    private async Task<TResponse> Request<T, TResponse>(T request, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, settings);

        logger.LogWarning("Sending request to Payu: {@Request}", json);

        var httpRequest = new HttpRequestMessage
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            Method = HttpMethod.Post
        };

        httpRequest.Headers.Add("Accept", "application/json");

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

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
