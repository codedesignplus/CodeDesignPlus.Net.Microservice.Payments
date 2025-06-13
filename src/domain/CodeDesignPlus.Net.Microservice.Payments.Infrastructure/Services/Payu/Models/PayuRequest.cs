using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

/// <summary>
/// Represents a request to the PayU payment gateway.
/// </summary>
public class PayuRequest
{
  /// <summary>
  /// Language used in the request, this language is used to display error messages generated.
  /// </summary>
  public string Language { get; set; } = null!;
  /// <summary>
  /// Assigns SUBMIT_TRANSACTION.
  /// </summary>
  public string Command { get; } = "SUBMIT_TRANSACTION";
  /// <summary>
  /// Assigns true if the request is in test mode. If not, assign false.
  /// </summary>
  public bool Test { get; set; } = false;
  /// <summary>
  /// This object contains the authentication data.
  /// </summary>
  public PayuMerchant Merchant { get; set; } = null!;
  /// <summary>
  /// This object contains the transaction data.
  /// </summary>
  public PayuTransaction Transaction { get; set; } = null!;
}

/// <summary>
/// Represents a merchant in the PayU payment gateway.
/// </summary>
public class PayuMerchant
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

/// <summary>
/// Represents a transaction request to the PayU payment gateway.
/// </summary>
public class PayuTransaction
{
  /// <summary>
  /// This object contains the order data.
  /// </summary>
  public PayuOrder Order { get; set; } = null!;
  /// <summary>
  /// Credit card token ID, include this parameter when the transaction is made with a tokenized credit card.
  /// </summary>
  public string? CreditCardTokenId { get; set; }
  /// <summary>
  /// Information about the credit card.
  /// </summary>
  public PayuCreditCard? CreditCard { get; set; }
  /// <summary>
  /// Information about the payer.
  /// </summary>
  public PayuPayer Payer { get; set; } = null!;
  /// <summary>
  /// Type of transaction, for Colombia, assign AUTHORIZATION_AND_CAPTURE.
  /// </summary>
  public string Type { get; set; } = "AUTHORIZATION_AND_CAPTURE";
  /// <summary>
  /// Payment method, select a valid credit card payment method.
  /// </summary>
  public string PaymentMethod { get; set; } = null!;// PaymentMethods.VISA; 
  /// <summary>
  /// Payment country, assign CO for Colombia.
  /// </summary>
  public string PaymentCountry { get; set; } = "CO";
  /// <summary>
  /// Identifier of the device session where the client performs the transaction.
  /// </summary>
  public string? DeviceSessionId { get; set; }
  /// <summary>
  /// IP address of the device where the client performs the transaction.
  /// </summary>
  public string? IpAddress { get; set; }
  /// <summary>
  /// Cookie stored by the device where the client performs the transaction.
  /// </summary>
  public string? Cookie { get; set; }
  /// <summary>
  /// User agent of the browser where the client performs the transaction.
  /// </summary>
  public string? UserAgent { get; set; }
  /// <summary>
  /// Additional parameters or data associated with the request.
  /// The maximum size of each extra parameter name is 64 characters.
  /// </summary>
  public Dictionary<string, string> ExtraParameters { get; set; } = [];
}

/// <summary>
/// Represents an order in the PayU payment gateway.
/// </summary>
public class PayuOrder
{
  /// <summary>
  /// Account identifier.
  /// </summary>
  public int AccountId { get; set; }
  /// <summary>
  /// Represents the identifier of the order in your system.
  /// </summary>
  public string ReferenceCode { get; set; } = null!;
  /// <summary>   
  /// Description of the order.
  /// </summary>
  public string Description { get; set; } = null!;
  /// <summary>
  /// Language used in emails sent to the buyer and seller.
  /// </summary>
  public string Language { get; set; } = "es";
  /// <summary>
  /// Confirmation URL for the order.
  /// </summary>
  public string? NotifyUrl { get; set; }
  /// <summary>
  /// ID of the partner within PayU.
  /// </summary>
  public string? PartnerId { get; set; }
  /// <summary>
  /// Signature associated with the form.
  /// </summary>
  public string Signature { get; set; } = null!;
  /// <summary>
  /// Shipping address.
  /// </summary>
  public PayuAddress? ShippingAddress { get; set; }
  /// <summary>
  /// Information about the buyer.
  /// </summary>
  public PayuBuyer Buyer { get; set; } = null!;
  /// <summary>
  /// Additional values associated with the order.
  /// </summary>
  public PayuAdditionalValues AdditionalValues { get; set; } = new PayuAdditionalValues();
}

/// <summary>
/// Represents an address in the PayU payment gateway.
/// </summary>
public class PayuAddress
{
  /// <summary>
  /// Line 1 of the shipping address.
  /// </summary>
  public string Street1 { get; set; } = null!;
  /// <summary>
  /// Line 2 of the shipping address.
  /// </summary>
  public string? Street2 { get; set; }
  /// <summary>
  /// City of the shipping address.
  /// </summary>
  public string City { get; set; } = null!;
  /// <summary>
  /// State of the shipping address.
  /// </summary>
  public string State { get; set; } = null!;
  /// <summary>
  /// Country of the shipping address in ISO 3166 alpha-2 format.
  /// </summary>
  public string Country { get; set; } = null!;
  /// <summary>
  /// Postal code of the shipping address.
  /// </summary>
  public string PostalCode { get; set; } = null!;
  /// <summary>
  /// Phone number associated with the shipping address.
  /// </summary>
  public string? Phone { get; set; }
}

/// <summary>
/// Represents a buyer in the PayU payment gateway.
/// </summary>
public class PayuBuyer
{
  /// <summary>
  /// Identifier of the buyer in your system.
  /// </summary>
  public string? MerchantBuyerId { get; set; }

  /// <summary>
  /// Full name of the buyer.
  /// </summary>
  public string FullName { get; set; } = null!;

  /// <summary>
  /// Email address of the buyer.
  /// </summary>
  public string EmailAddress { get; set; } = null!;

  /// <summary>
  /// Contact phone number of the buyer.
  /// </summary>
  public string ContactPhone { get; set; } = null!;

  /// <summary>
  /// Identification number of the buyer.
  /// </summary>
  public string? DniNumber { get; set; }

  /// <summary>
  /// Shipping address of the buyer.
  /// </summary>
  public PayuAddress ShippingAddress { get; set; } = null!;
}

/// <summary>
/// Represents additional values associated with a transaction in the PayU payment gateway.
/// </summary>
public class PayuAdditionalValues
{
  /// <summary>
  /// Transaction amount.
  /// </summary>
  [JsonProperty(PropertyName = "TX_VALUE")]
  public PayuAmount Value { get; set; } = new PayuAmount();
  /// <summary>
  /// VAT (Value Added Tax) amount.
  /// </summary>
  [JsonProperty(PropertyName = "TX_TAX")]
  public PayuAmount Tax { get; set; } = new PayuAmount();
  /// <summary>
  /// Base value for calculating VAT.
  /// </summary>
  [JsonProperty(PropertyName = "TX_TAX_RETURN_BASE")]
  public PayuAmount TaxReturnBase { get; set; } = new PayuAmount();
}

/// <summary>
/// Represents an amount in the PayU payment gateway.
/// </summary>
public class PayuAmount
{
  /// <summary>
  /// Specifies the amount of the transaction, this value cannot have decimals.
  /// </summary>
  public long Value { get; set; }
  /// <summary>
  /// ISO code of the currency.
  /// </summary>
  public string Currency { get; set; } = "COP";
}

/// <summary>
/// Represents a credit card in the PayU payment gateway.
/// </summary>
public class PayuCreditCard
{
  /// <summary>
  /// Credit card number.
  /// </summary>
  public string Number { get; set; } = null!;

  /// <summary>
  /// Security code of the credit card (CVC2, CVV2, CID).
  /// </summary>
  public string SecurityCode { get; set; } = null!;

  /// <summary>
  /// Expiration date of the credit card in YYYY/MM format.
  /// </summary>
  public string ExpirationDate { get; set; } = null!;

  /// <summary>
  /// Name of the cardholder as shown on the credit card.
  /// </summary>
  public string Name { get; set; } = null!;
  /// <summary>
  /// Allows processing transactions without including the security code of the credit card.
  /// Your business requires authorization from PayU before using this functionality.
  /// </summary>
  public bool ProcessWithoutCvv2 { get; set; } = false;
}

/// <summary>
/// Represents a payer in the PayU payment gateway.
/// </summary>
public class PayuPayer
{
  /// <summary>
  /// Email address of the payer.
  /// </summary>
  public string EmailAddress { get; set; } = null!;

  /// <summary>
  /// Identifier of the payer in your system.
  /// </summary>
  public string? MerchantPayerId { get; set; }

  /// <summary>
  /// Full name of the payer, which must match the name on the credit card.
  /// </summary>
  public string FullName { get; set; } = null!;

  /// <summary>
  /// Billing address of the payer.
  /// </summary>
  [Required]
  public PayuAddress BillingAddress { get; set; } = null!;

  /// <summary>
  /// Birthdate of the payer.
  /// </summary>
  public string? Birthdate { get; set; }

  /// <summary>
  /// Contact phone number of the payer.
  /// </summary>
  public string ContactPhone { get; set; } = null!;

  /// <summary>
  /// Identification number of the payer.
  /// </summary>
  public string? DniNumber { get; set; }

  /// <summary>
  /// Type of identification of the payer.
  /// </summary>
  public string? DniType { get; set; } = "CC";
}