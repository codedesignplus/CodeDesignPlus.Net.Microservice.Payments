using System;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;


/// <summary>
/// Represents the response from the payment gateway after attempting to tokenize a credit card.
/// </summary>
public class CreditCardTokenResponse
{
    /// <summary>
    /// The response code from the payment gateway.
    /// </summary>
    public string Code { get; set; } = null!;
    /// <summary>
    /// The error message from the payment gateway, if any.
    /// </summary>
    public string Error { get; set; } = null!;
    /// <summary>
    /// The tokenized credit card information.
    /// </summary>
    public TokenizeCardResponseDto CreditCardToken { get; set; } = null!;
}
