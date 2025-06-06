namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Constants;

/// <summary>
/// Represents the available payment methods in the PayU payment gateway for Colombia.
/// </summary>
public class PayuPaymentMethods
{
  /// <summary>
  /// American Express payment method.
  /// </summary>
  public const string AMEX = "AMEX";
  /// <summary>
  /// Banco de Bogotá payment method.
  /// </summary>
  public const string BANK_REFERENCED = "BANK_REFERENCED";
  /// <summary>
  /// Bancolombia payment method.
  /// </summary>
  public const string BANCOLOMBIA_BUTTON = "BANCOLOMBIA_BUTTON";
  /// <summary>
  /// Codensa payment method.
  /// </summary>
  public const string CODENSA = "CODENSA";
  /// <summary>
  /// Davivienda payment method.
  /// </summary>
  public const string DINERS = "DINERS";
  /// <summary>
  /// Efecty payment method.
  /// </summary>
  public const string EFECTY = "EFECTY";
  /// <summary>
  /// Google Pay payment method.
  /// </summary>
  public const string GOOGLE_PAY = "GOOGLE_PAY";
  /// <summary>
  /// Mastercard payment method.
  /// </summary>
  public const string MASTERCARD = "MASTERCARD";
  /// <summary>
  /// Mastercard debit payment method.
  /// </summary>
  public const string NEQUI = "NEQUI";
  /// <summary>
  /// PSE payment method.
  /// </summary>
  public const string OTHERS_CASH = "OTHERS_CASH";
  /// <summary>
  /// PSE payment method.
  /// </summary>
  public const string PSE = "PSE";
  /// <summary>
  /// Su Red payment method.
  /// </summary>
  public const string VISA = "VISA";
}