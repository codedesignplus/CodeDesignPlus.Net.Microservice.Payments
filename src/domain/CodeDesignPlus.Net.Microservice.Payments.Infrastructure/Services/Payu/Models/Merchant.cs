using System;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

/// <summary>
/// Represents a merchant in the PayU payment gateway.
/// </summary>
public class Merchant
{
  /// <summary>
  /// User or login provided by PayU. How to get my API Login.
  /// </summary>  
  public string ApiLogin { get; set; } = null!;
  /// <summary>
  /// Password provided by PayU. How to get my API Key.
  /// </summary>
  public string ApiKey { get; set; } = null!;
}