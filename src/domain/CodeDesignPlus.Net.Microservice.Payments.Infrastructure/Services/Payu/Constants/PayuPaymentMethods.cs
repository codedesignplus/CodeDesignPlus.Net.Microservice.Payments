using System.Collections.ObjectModel;
using CodeDesignPlus.Net.Exceptions.Guards;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Constants;

/// <summary>
/// Represents the available payment methods in the PayU payment gateway for Colombia.
/// </summary>
public class PayuPaymentMethods
{
  private static readonly ReadOnlyCollection<PaymentMethod> paymentMethods = new(
  [
    new PaymentMethod { Name = "American Express", Code = AMEX, Type = PayType.CreditCard },
    new PaymentMethod { Name = "Banco de Bogotá", Code = BANK_REFERENCED, Type = PayType.Cash },
    new PaymentMethod { Name = "Bancolombia Button", Code = BANCOLOMBIA_BUTTON, Type = PayType.Cash },
    new PaymentMethod { Name = "Codensa", Code = CODENSA, Type = PayType.Cash },
    new PaymentMethod { Name = "Diners", Code = DINERS, Type = PayType.CreditCard },
    new PaymentMethod { Name = "Efecty", Code = EFECTY, Type = PayType.Cash },
    new PaymentMethod { Name = "Google Pay", Code = GOOGLE_PAY, Type = PayType.CreditCard },
    new PaymentMethod { Name = "Mastercard", Code = MASTERCARD, Type = PayType.CreditCard },
    new PaymentMethod { Name = "Nequi", Code = NEQUI, Type = PayType.DebitCard },
    new PaymentMethod { Name = "Others Cash", Code = OTHERS_CASH, Type = PayType.Cash },
    new PaymentMethod { Name = "PSE", Code = PSE, Type = PayType.Pse },
    new PaymentMethod { Name = "Visa", Code = VISA, Type = PayType.CreditCard }
  ]);

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

  /// <summary>
  /// Checks if the provided payment method code is valid.
  /// Valid payment methods include: AMEX, BANK_REFERENCED, BANCOLOMBIA_BUTTON, CODENSA, DINERS, EFECTY, GOOGLE_PAY, MASTERCARD, NEQUI, OTHERS_CASH, PSE, and VISA.
  /// </summary>
  /// <param name="code">The payment method code to validate.</param>
  /// <returns> Returns true if the code is a valid payment method; otherwise, false.</returns>
  public static bool IsValidPaymentMethod(string code)
  {
    return code switch
    {
      AMEX or BANK_REFERENCED or BANCOLOMBIA_BUTTON or CODENSA or DINERS or EFECTY or GOOGLE_PAY or MASTERCARD or NEQUI or OTHERS_CASH or PSE or VISA => true,
      _ => false
    };
  }

  /// <summary>
  /// Gets the payment method by its code.
  /// </summary>
  /// <param name="code">The payment method code to retrieve.</param>
  /// <returns>Returns the PaymentMethod object corresponding to the provided code.</returns>
  public static PaymentMethod GetPaymentMethod(string code)
  {
    InfrastructureGuard.IsFalse(IsValidPaymentMethod(code), $"The payment method code '{code}' is not valid.");

    return paymentMethods.First(pm => pm.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
  }
}

public class PaymentMethod
{
  public string Name { get; set; } = null!;
  public string Code { get; set; } = null!;
  public PayType Type { get; set; } 

}

public enum PayType
{
  CreditCard,
  DebitCard,
  Pse,
  Cash
}