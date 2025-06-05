using System;
using System.ComponentModel.DataAnnotations;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Models;

public class PaymentRequest
{
    /// <summary>
    /// Language used in the request, this language is used to display error messages generated.
    /// </summary>
    [Required]
    public string Language { get; set; } = null!;
    /// <summary>
    /// Assigns SUBMIT_TRANSACTION.
    /// </summary>
    [Required]
    [RegularExpression(@"^[A-Z_]+$", ErrorMessage = "Command must be in uppercase and contain only letters and underscores.")]
    [StringLength(32, MinimumLength = 1, ErrorMessage = "Command must be between 1 and 32 characters long.")]
    public string Command { get; set; } = "SUBMIT_TRANSACTION";
    /// <summary>
    /// Assigns true if the request is in test mode. If not, assign false.
    /// </summary>
    public bool IsTest { get; set; } = false;
    /// <summary>
    /// This object contains the authentication data.
    /// </summary>
    [Required]
    public Merchant Merchant { get; set; } = null!;
    /// <summary>
    /// This object contains the transaction data.
    /// </summary>
    [Required]
    public Transaction Transaction { get; set; } = null!;
}

public class Merchant
{
    /// <summary>
    /// User or login provided by PayU. How to get my API Login.
    /// </summary>
    [Required]
    [StringLength(32, MinimumLength = 12, ErrorMessage = "API Login must be between 12 and 32 characters long.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "API Login must contain only letters, numbers, and underscores.")]
    public string ApiLogin { get; set; } = null!;
    /// <summary>
    /// Password provided by PayU. How to get my API Key.
    /// </summary>
    [Required]
    [StringLength(32, MinimumLength = 6, ErrorMessage = "API Key must be between 6 and 32 characters long.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "API Key must contain only letters, numbers, and underscores.")]
    public string ApiKey { get; set; } = null!;
}

public class Transaction
{
    /// <summary>
    /// This object contains the order data.
    /// </summary>
    [Required]
    public Order Order { get; set; } = null!;
    /// <summary>
    /// Credit card token ID, include this parameter when the transaction is made with a tokenized credit card.
    /// </summary>
    public string? CreditCardTokenId { get; set; }
    /// <summary>
    /// Information about the credit card.
    /// </summary>
    public CreditCard? CreditCard { get; set; }
    /// <summary>
    /// Information about the payer.
    /// </summary>
    [Required]
    public Payer Payer { get; set; } = null!;
    /// <summary>
    /// Type of transaction, for Colombia, assign AUTHORIZATION_AND_CAPTURE.
    /// </summary>
    [Required]
    [RegularExpression(@"^[A-Z_]+$", ErrorMessage = "Type must be in uppercase and contain only letters and underscores.")]
    [StringLength(32, MinimumLength = 1, ErrorMessage = "Type must be between 1 and 32 characters long.")]
    public string Type { get; set; } = "AUTHORIZATION_AND_CAPTURE";
    /// <summary>
    /// Payment method, select a valid credit card payment method.
    /// </summary>
    [Required]
    [StringLength(32, MinimumLength = 1, ErrorMessage = "Payment method must be between 1 and 32 characters long.")]
    public string PaymentMethod { get; set; } = null!;// PaymentMethods.VISA; 
    /// <summary>
    /// Payment country, assign CO for Colombia.
    /// </summary>
    [Required]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Payment country must be a two-letter uppercase ISO 3166-1 alpha-2 code.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Payment country must be exactly 2 characters long.")]
    public string PaymentCountry { get; set; } = "CO";
    /// <summary>
    /// Identifier of the device session where the client performs the transaction.
    /// </summary>
    [Required]
    [StringLength(255, ErrorMessage = "Device session ID cannot exceed 255 characters.")]
    public string? DeviceSessionId { get; set; }
    /// <summary>
    /// IP address of the device where the client performs the transaction.
    /// </summary>
    [Required]
    [StringLength(39, ErrorMessage = "IP address cannot exceed 39 characters.")]
    [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}(:\d{1,5})?$", ErrorMessage = "IP address must be a valid IPv4 or IPv6 address.")]
    public string? IpAddress { get; set; }
    /// <summary>
    /// Cookie stored by the device where the client performs the transaction.
    /// </summary>
    [Required]
    [StringLength(255, ErrorMessage = "Cookie cannot exceed 255 characters.")]
    public string? Cookie { get; set; }
    /// <summary>
    /// User agent of the browser where the client performs the transaction.
    /// </summary>
    [Required]
    [StringLength(1024, ErrorMessage = "User agent cannot exceed 1024 characters.")]
    public string? UserAgent { get; set; }
    /// <summary>
    /// Additional parameters or data associated with the request.
    /// The maximum size of each extra parameter name is 64 characters.
    /// </summary>
    public Dictionary<string, string> ExtraParameters { get; set; } = [];

}
public class Order
{
    /// <summary>
    /// Account identifier.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Account ID must be a positive integer.")]
    public int AccountId { get; set; }
    /// <summary>
    /// Represents the identifier of the order in your system.
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Reference code must be between 1 and 255 characters long.")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Reference code must contain only letters, numbers, underscores, and hyphens.")]
    public string ReferenceCode { get; set; } = null!;
    /// <summary>
    /// Description of the order.
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 255 characters long.")]
    public string Description { get; set; } = null!;
    /// <summary>
    /// Language used in emails sent to the buyer and seller.
    /// </summary>
    [Required]
    [RegularExpression(@"^[a-z]{2}$", ErrorMessage = "Language must be a two-letter lowercase ISO 639-1 code.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Language must be exactly 2 characters long.")]
    public string Language { get; set; } = "es";
    /// <summary>
    /// Confirmation URL for the order.
    /// </summary>
    [StringLength(2048, ErrorMessage = "Notify URL cannot exceed 2048 characters.")]
    [Url(ErrorMessage = "Notify URL must be a valid URL.")]
    [RegularExpression(@"^https?://", ErrorMessage = "Notify URL must start with http:// or https://.")]
    public string? NotifyUrl { get; set; }
    /// <summary>
    /// ID of the partner within PayU.
    /// </summary>
    [StringLength(255, ErrorMessage = "Partner ID cannot exceed 255 characters.")]
    public string? PartnerId { get; set; }
    /// <summary>
    /// Signature associated with the form.
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Signature must be between 1 and 255 characters long.")]
    public string Signature { get; set; } = null!;
    /// <summary>
    /// Shipping address.
    /// </summary>
    public Address? ShippingAddress { get; set; }
    /// <summary>
    /// Information about the buyer.
    /// </summary>
    [Required]
    public Buyer Buyer { get; set; } = null!;
    /// <summary>
    /// Additional values associated with the order.
    /// </summary>
    [Required]
    public AdditionalValues AdditionalValues { get; set; } = new AdditionalValues();
}

public class Address
{
    /// <summary>
    /// Line 1 of the shipping address.
    /// </summary>
    [StringLength(100, ErrorMessage = "Street1 cannot exceed 100 characters.")]
    public string Street1 { get; set; } = null!;
    /// <summary>
    /// Line 2 of the shipping address.
    /// </summary>
    [StringLength(100, ErrorMessage = "Street2 cannot exceed 100 characters.")]
    public string? Street2 { get; set; }
    /// <summary>
    /// City of the shipping address.
    /// </summary>
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
    public string City { get; set; } = null!;
    /// <summary>
    /// State of the shipping address.
    /// </summary>
    [StringLength(40, ErrorMessage = "State cannot exceed 40 characters.")]
    public string State { get; set; } = null!;
    /// <summary>
    /// Country of the shipping address in ISO 3166 alpha-2 format.
    /// </summary>
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country must be a two-letter uppercase ISO 3166-1 alpha-2 code.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country must be exactly 2 characters long.")]
    public string Country { get; set; } = null!;
    /// <summary>
    /// Postal code of the shipping address.
    /// </summary>
    [StringLength(8, ErrorMessage = "Postal code cannot exceed 8 characters.")]
    [RegularExpression(@"^\d{1,8}$", ErrorMessage = "Postal code must be numeric and up to 8 digits long.")]
    public string PostalCode { get; set; } = null!;
    /// <summary>
    /// Phone number associated with the shipping address.
    /// </summary>
    [StringLength(11, ErrorMessage = "Phone number cannot exceed 11 characters.")]
    [RegularExpression(@"^\+?\d{1,11}$", ErrorMessage = "Phone number must be numeric and can include a leading + sign.")]
    public string? Phone { get; set; }
}


public class PayerBillingAddress
{
    /// <summary>
    /// Line 1 of the shipping address.
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "Street1 cannot exceed 100 characters.")]
    public string Street1 { get; set; } = null!;
    /// <summary>
    /// Line 2 of the shipping address.
    /// </summary>
    [StringLength(100, ErrorMessage = "Street2 cannot exceed 100 characters.")]
    public string? Street2 { get; set; }
    /// <summary>
    /// City of the shipping address.
    /// </summary>
    [Required]
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
    public string City { get; set; } = null!;
    /// <summary>
    /// State of the shipping address.
    /// </summary>
    [StringLength(40, ErrorMessage = "State cannot exceed 40 characters.")]
    public string State { get; set; } = null!;
    /// <summary>
    /// Country of the shipping address in ISO 3166 alpha-2 format.
    /// </summary>
    [Required]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country must be a two-letter uppercase ISO 3166-1 alpha-2 code.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country must be exactly 2 characters long.")]
    public string Country { get; set; } = null!;
    /// <summary>
    /// Postal code of the shipping address.
    /// </summary>
    [StringLength(8, ErrorMessage = "Postal code cannot exceed 8 characters.")]
    [RegularExpression(@"^\d{1,8}$", ErrorMessage = "Postal code must be numeric and up to 8 digits long.")]
    public string PostalCode { get; set; } = null!;
    /// <summary>
    /// Phone number associated with the shipping address.
    /// </summary>
    [StringLength(11, ErrorMessage = "Phone number cannot exceed 11 characters.")]
    [RegularExpression(@"^\+?\d{1,11}$", ErrorMessage = "Phone number must be numeric and can include a leading + sign.")]
    public string? Phone { get; set; }

}

public class Buyer
{
    /// <summary>
    /// Identifier of the buyer in your system.
    /// </summary>
    [StringLength(100, ErrorMessage = "MerchantBuyerId cannot exceed 100 characters.")]
    public string? MerchantBuyerId { get; set; }
    /// <summary>
    /// Full name of the buyer.
    /// </summary>
    [Required]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Full name must be between 1 and 150 characters long.")]
    public string FullName { get; set; } = null!;
    /// <summary>
    /// Email address of the buyer.
    /// </summary>
    [Required]
    [StringLength(255, ErrorMessage = "Email address cannot exceed 255 characters.")]
    [EmailAddress(ErrorMessage = "Email address must be a valid email format.")]
    public string EmailAddress { get; set; } = null!;
    /// <summary>
    /// Contact phone number of the buyer.
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters.")]
    [RegularExpression(@"^\+?\d{1,20}$", ErrorMessage = "Contact phone must be numeric and can include a leading + sign.")]
    public string ContactPhone { get; set; } = null!;
    /// <summary>
    /// Identification number of the buyer.
    /// </summary>
    [StringLength(20, ErrorMessage = "DniNumber cannot exceed 20 characters.")]
    public string? DniNumber { get; set; }
    /// <summary>
    /// Shipping address of the buyer.
    /// </summary>
    [Required]
    public Address ShippingAddress { get; set; } = null!;
}

public class AdditionalValues
{
    /// <summary>
    /// Transaction amount.
    /// </summary>
    [Required]
    public Amount TX_VALUE { get; set; } = new Amount();
    /// <summary>
    /// VAT (Value Added Tax) amount.
    /// </summary>
    [Required]
    public Amount TX_TAX { get; set; } = new Amount();
    /// <summary>
    /// Base value for calculating VAT.
    /// </summary>
    [Required]
    public Amount TX_TAX_RETURN_BASE { get; set; } = new Amount();
}
public class Amount
{
    /// <summary>
    /// Specifies the amount of the transaction, this value cannot have decimals.
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Value must be a positive number.")]
    public long Value { get; set; }
    /// <summary>
    /// ISO code of the currency.
    /// </summary>
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be a three-letter uppercase ISO 4217 code.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be exactly 3 characters long.")]
    public string Currency { get; set; } = "COP";
}

public class CreditCard
{
    /// <summary>
    /// Credit card number.
    /// </summary>
    [StringLength(20, MinimumLength = 13, ErrorMessage = "Card number must be between 13 and 20 characters long.")]
    public string Number { get; set; } = null!;
    /// <summary>
    /// Security code of the credit card (CVC2, CVV2, CID).
    /// </summary>
    [StringLength(4, MinimumLength = 1, ErrorMessage = "Security code must be between 1 and 4 characters long.")]
    public string SecurityCode { get; set; } = null!;
    /// <summary>
    /// Expiration date of the credit card in YYYY/MM format.
    /// </summary>
    [StringLength(7, MinimumLength = 7, ErrorMessage = "Expiration date must be in YYYY/MM format and exactly 7 characters long.")]
    [RegularExpression(@"^\d{4}/\d{2}$", ErrorMessage = "Expiration date must be in YYYY/MM format.")]
    public string ExpirationDate { get; set; } = null!;
    /// <summary>
    /// Name of the cardholder as shown on the credit card.
    /// </summary>
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Cardholder name must be between 1 and 255 characters long.")]
    public string Name { get; set; } = null!;
    /// <summary>
    /// Allows processing transactions without including the security code of the credit card.
    /// Your business requires authorization from PayU before using this functionality.
    /// </summary>
    public bool ProcessWithoutCvv2 { get; set; } = false;
}
public class Payer
{
    /// <summary>
    /// Email address of the payer.
    /// </summary>
    [Required]
    [StringLength(255, ErrorMessage = "Email address cannot exceed 255 characters.")]
    [EmailAddress(ErrorMessage = "Email address must be a valid email format.")]
    public string EmailAddress { get; set; } = null!;
    /// <summary>
    /// Identifier of the payer in your system.
    /// </summary>
    [StringLength(100, ErrorMessage = "MerchantPayerId cannot exceed 100 characters.")]
    public string? MerchantPayerId { get; set; }
    /// <summary>
    /// Full name of the payer, which must match the name on the credit card.
    /// </summary>
    [Required]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Full name must be between 1 and 150 characters long.")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name must contain only letters and spaces.")]
    public string FullName { get; set; } = null!;
    /// <summary>
    /// Billing address of the payer.
    /// </summary>
    [Required]
    public PayerBillingAddress BillingAddress { get; set; } = null!;
    /// <summary>
    /// Birthdate of the payer.
    /// </summary>
    [StringLength(10, ErrorMessage = "Birthdate must be in YYYY-MM-DD format and exactly 10 characters long.")]
    public string? Birthdate { get; set; }
    /// <summary>
    /// Contact phone number of the payer.
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters.")]
    [RegularExpression(@"^\+?\d{1,20}$", ErrorMessage = "Contact phone must be numeric and can include a leading + sign.")]
    public string ContactPhone { get; set; } = null!;
    /// <summary>
    /// Identification number of the payer.
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "DniNumber cannot exceed 20 characters.")]
    public string? DniNumber { get; set; }
    /// <summary>
    /// Type of identification of the payer.
    /// </summary>
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "DniType must be a two-letter uppercase code.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "DniType must be exactly 2 characters long.")]
    public string? DniType { get; set; } = "CC"; // Default to CC (Cédula de Ciudadanía)
}


public class PaymentMethods
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

/*
language	Alfanumérico	2	Idioma utilizado en la petición, este idioma se utiliza para mostrar los mensajes de error generados. Ver idiomas soportados.	Sí
command	Alfanumérico	Máx:32	Asigna SUBMIT_TRANSACTION.	Sí
test (JSON)
isTest (XML)	Booleano		Asigna true si la petición es en modo pruebas. Si no, asigna false.	Sí
merchant	Objeto		Este objeto contiene los datos de autenticación.	Sí
merchant > apiLogin	Alfanumérico	Min:12 Máx:32	Usuario o login entregado por PayU. Cómo obtengo mi API Login	Sí
merchant > apiKey	Alfanumérico	Min:6 Máx:32	Contraseña entregada por PayU. Cómo obtengo mi API Key	Sí
transaction	Objeto		Este objeto contiene los datos de la transacción.	Sí
transaction > order	Objeto		Este objeto contiene los datos de la orden.	Sí
transaction > order > accountId	Numérico		Identificador de tu cuenta.	Sí
transaction > order > referenceCode	Alfanumérico	Min:1 Máx:255	Representa el identificador de la orden en tu sistema.	Sí
transaction > order > description	Alfanumérico	Min:1 Máx:255	Descripción de la orden.	Sí
transaction > order > language	Alfanumérico	2	Idioma utilizado en los correos electrónicos enviados al comprador y al vendedor.	Sí
transaction > order > notifyUrl	Alfanumérico	Máx:2048	URL de confirmación de la orden.	No
transaction > order > partnerId	Alfanumérico	Máx:255	ID de aliado dentro de PayUID de aliado dentro de PayU.	No
transaction > order > signature	Alfanumérico	Máx:255	Firma asociada al formulario. Para más información, consulta Firma de autenticación.	Sí
transaction > order > shippingAddress	Objeto		Dirección de envío.	No
transaction > order > shippingAddress > street1	Alfanumérico	Máx:100	Línea de dirección 1.	No
transaction > order > shippingAddress > street2	Alfanumérico	Máx:100	Línea de dirección 2.	No
transaction > order > shippingAddress > city	Alfanumérico	Máx:50	Ciudad de la dirección.	No
transaction > order > shippingAddress > state	Alfanumérico	Máx:40	Departamento de la dirección.	No
transaction > order > shippingAddress > country	Alfanumérico	2	País de la dirección.	No
transaction > order > shippingAddress > postalCode	Alfanumérico	Máx:8	Código postal de la dirección.	No
transaction > order > shippingAddress > phone	Alfanumérico	Máx:11	Número de teléfono asociado con la dirección.	No
transaction > order > buyer	Objeto		Información del comprador.	Sí
transaction > order > buyer > merchantBuyerId	Alfanumérico	Máx:100	Identificador del comprador en tu sistema.	No
transaction > order > buyer > fullName	Alfanumérico	Máx:150	Nombre del comprador.	Sí
transaction > order > buyer > emailAddress	Alfanumérico	Máx:255	Correo electrónico del comprador.	Sí
transaction > order > buyer > contactPhone	Alfanumérico	Máx:20	Número de teléfono del comprador.	Sí
transaction > order > buyer > dniNumber	Alfanumérico	Máx:20	Número de identificación del comprador.	Sí
transaction > order > buyer > shippingAddress	Objeto		Dirección de envío del comprador.	Sí
transaction > order > buyer > shippingAddress > street1	Alfanumérico	Máx:150	Línea de dirección 1 del comprador.	Sí
transaction > order > buyer > shippingAddress > city	Alfanumérico	Máx:50	Ciudad de la dirección del comprador.	Sí
transaction > order > buyer > shippingAddress > state	Alfanumérico	Máx:40	Departamento de la dirección del comprador.	Sí
transaction > order > buyer > shippingAddress > country	Alfanumérico	2	País de la dirección del comprador en formato ISO 3166 alpha-2.	Sí
transaction > order > buyer > shippingAddress > postalCode	Numérico	Máx:20	Código postal de la dirección del comprador.	Sí
transaction > order > buyer > shippingAddress > phone	Numérico	Máx:20	Número de teléfono de la dirección del comprador.	Sí
transaction > order > additionalValues >	Objeto	64	Monto de la orden y sus valores asociados.	Sí
transaction > order > additionalValues > TX_VALUE	Alfanumérico	64	Monto de la transacción.	Sí
transaction > order > additionalValues > TX_VALUE > value	Numérico	12, 2	Especifica el monto de la transacción, este valor no puede tener decimales.	Sí
transaction > order > additionalValues > TX_VALUE > currency	Alfanumérico	3	Código ISO de la moneda. Ver monedas aceptadas.	No
transaction > order > additionalValues > TX_TAX	Alfanumérico	64	Monto del IVA (Impuesto al Valor Agregado).	Sí
transaction > order > additionalValues > TX_TAX > value	Numérico	12, 2	Especifica el monto del IVA.
Si no se envía este parámetro, PayU aplica el impuesto actual (19%).
Si la cantidad está exenta de IVA, envía 0.
Este valor puede tener dos dígitos decimales	No
transaction > order > additionalValues > TX_TAX > currency	Alfanumérico	3	Código ISO de la moneda. Ver monedas aceptadas.	No
transaction > order > additionalValues > TX_TAX_RETURN_BASE	Alfanumérico	64	Valor base para calcular el IVA.
Si la cantidad está exenta de IVA, envía 0.
Este valor puede tener dos dígitos decimales	No
transaction > order > additionalValues > TX_TAX_RETURN_BASE > value	Numérico	12, 2	Especifica el valor base de la transacción.	No
transaction > order > additionalValues > TX_TAX_RETURN_BASE > currency	Alfanumérico	3	Código ISO de la moneda. Ver monedas aceptadas.	No
transaction > creditCardTokenId	Alfanumérico		Incluye este parámetro cuando la transacción se hace con una tarjeta de crédito tokenizada; además, es obligatorio enviar el parámetro transaction.creditCard.expirationDate.
Para más información, consulte API de Tokenización.	No
transaction > creditCard	Objeto		Información de la tarjeta de crédito. Este objeto y sus parámetros son obligatorios cuando el pago se realiza utilizando una tarjeta de crédito no tokenizada.	No
transaction > creditCard > number	Alfanumérico	Min:13 Máx:20	Número de la tarjeta de crédito.	No
transaction > creditCard > securityCode	Alfanumérico	Min:1 Máx:4	Código de seguridad de la tarjeta de crédito (CVC2, CVV2, CID).	No
transaction > creditCard > expirationDate	Alfanumérico	7	Fecha de expiración de la tarjeta de crédito. Formato YYYY/MM.	No
transaction > creditCard > name	Alfanumérico	Min:1 Máx:255	Nombre del tarjetahabiente mostrado en la tarjeta de crédito.	No
transaction > creditCard > processWithoutCvv2	Booleano	Máx:255	Te permite procesar transacciones sin incluir el código de seguridad de la tarjeta de crédito. Tu comercio requiere autorización de PayU antes de utilizar esta funcionalidad.	No
transaction > payer	Objeto		Información del pagador.	Sí
transaction > payer > emailAddress	Alfanumérico	Máx:255	Correo electrónico del pagador.	Sí
transaction > payer > merchantPayerId	Alfanumérico	Máx:100	Identificador del pagador en tu sistema.	No
transaction > payer > fullName	Alfanumérico	Máx:150	Nombre del pagador que debe ser igual al enviado en el parámetro transaction.creditCard.name.	Sí
transaction > payer > billingAddress	Objeto		Dirección de facturación.	Sí
transaction > payer > billingAddress > street1	Alfanumérico	Máx:100	Línea 1 de la dirección de facturación.	Sí
transaction > payer > billingAddress > street2	Alfanumérico	Máx:100	Línea 2 de la dirección de facturación.	No
transaction > payer > billingAddress > city	Alfanumérico	Máx:50	Ciudad de la dirección de facturación.	Sí
transaction > payer > billingAddress > state	Alfanumérico	Máx:40	Departamento de la dirección de facturación.	No
transaction > payer > billingAddress > country	Alfanumérico	2	País de la dirección de facturación en formato ISO 3166 Alpha-2.	Sí
transaction > payer > billingAddress > postalCode	Alfanumérico	Máx:20	Código postal de la dirección de facturación.	No
transaction > payer > billingAddress > phone	Alfanumérico	Máx:20	Número de teléfono de la dirección de facturación.	No
transaction > payer > birthdate	Alfanumérico	Máx:10	Fecha de nacimiento del pagador.	No
transaction > payer > contactPhone	Alfanumérico	Máx:20	Número de teléfono del pagador.	Sí
transaction > payer > dniNumber	Alfanumérico	Máx:20	Número de identificación del pagador.	Sí
transaction > payer > dniType	Alfanumérico	2	Tipo de identificación del pagador. Ver tipos de documentos.	No
transaction > type	Alfanumérico	32	Asigna este valor de acuerdo con el tipo de transacción. Para Colombia, asigna AUTHORIZATION_AND_CAPTURE	Sí
transaction > paymentMethod	Alfanumérico	32	Selecciona un método de pago de Tarjeta de crédito valido. Ver los métodos de pago disponibles para Colombia.	Sí
transaction > paymentCountry	Alfanumérico	2	Asigna CO para Colombia.	Sí
transaction > deviceSessionId	Alfanumérico	Máx:255	Identificador de la sesión del dispositivo donde el cliente realiza la transacción. Para más información, consulta esta sección.	Sí
transaction > ipAddress	Alfanumérico	Máx:39	Dirección IP del dispositivo donde el cliente realiza la transacción.	Sí
transaction > cookie	Alfanumérico	Máx:255	Cookie almacenada por el dispositivo donde el cliente realiza la transacción.	Sí
transaction > userAgent	Alfanumérico	Máx:1024	User agent del navegador donde el cliente realiza la transacción.	Sí
transaction > extraParameters	Objeto		Parámetros adicionales o datos asociados a la petición. El tamaño máximo de cada nombre de extraParameters es 64 caracteres.
En JSON, el parámetro extraParameters sigue esta estructura:
"extraParameters": {
 "INSTALLMENTS_NUMBER": 1
}

En XML, el parámetro extraParameters sigue esta estructura:
<extraParameters>
 <entry>
  <string>INSTALLMENTS_NUMBER</string>
  <string>1</string>
 </entry>
</extraParameters>	No
transaction > extraParameters > EXTRA1	Alfanumérico	Máx:255	Campo adicional para enviar información extra sobre la compra.	No
transaction > extraParameters > EXTRA2	Alfanumérico	Máx:255	Campo adicional para enviar información extra sobre la compra.	No
transaction > extraParameters > EXTRA3	Alfanumérico	Máx:255	Campo adicional para enviar información extra sobre la compra.	No
transaction > threeDomainSecure	Objeto		Este objeto contiene la información de 3DS 2.0.	No
transaction > threeDomainSecure > embedded	Booleano		Asigna true si quieres utilizar un MPI embebido para el proceso de Autorización. Por defecto, este valor está asignado como false.	No
transaction > threeDomainSecure > eci	Numérico	Máx:2	Indicador de Comercio Electrónico.
Valor retornado por los servidores de directorio indicando el intento de autenticación.
Este parámetro es obligatorio cuando transaction.threeDomainSecure.embedded es false y transaction.threeDomainSecure.xid tiene un valor configurado.	No
transaction > threeDomainSecure > cavv	Alfanumérico	Máx:28	Valor de verificación de autenticación del titular de la tarjeta (Cardholder Authentication Verification Value).
Código del criptograma utilizado en la autenticación de la transacción codificado en Base 64.
Dependiendo de los códigos ECI específicos establecidos por la red, este valor puede ser opcional.	No
transaction > threeDomainSecure > xid	Alfanumérico	Máx:28	Identificador de la transacción enviado por el MPI codificado en Base 64.
Este parámetro es obligatorio cuando transaction.threeDomainSecure.embedded is false y transaction.threeDomainSecure.eci tiene un valor configurado.	No
transaction > threeDomainSecure > directoryServerTransactionId	Alfanumérico	Máx:36	Identificador de la transacción generador por el servidor de directorio durante la autenticación.	No
transaction > digitalWallet	Objeto		Incluya este parámetro cuando la transacción se realice utilizando una billetera digital. *Al enviar este objeto, todos sus campos son obligatorios.	No
transaction > digitalWallet > type	Alfanumérico	—-	Envía el valor con base en la billetera que se está procesando: GOOGLE_PAY	Si*
transaction > digitalWallet > message	Alfanumérico	—-	Incluye la información del Google Pay token que Google te devolverá por cada transacción. Para más información consulta aquí.	Si*
*/



// PaymentMethods
/*
American Express	AMEX	Tarjeta de crédito	
Logo	Banco de Bogotá	BANK_REFERENCED	Referencia bancaria	
Logo	Bancolombia	BANK_REFERENCED	Referencia bancaria	
Logo	Botón Bancolombia	BANCOLOMBIA_BUTTON	Transferencia bancaria	
Logo	Codensa	CODENSA	Tarjeta de crédito	
Logo	Davivienda	BANK_REFERENCED	Referencia bancaria	
Logo	Diners	DINERS	Tarjeta de crédito	
Logo	Efecty	EFECTY	Efectivo	
Logo	Google Pay	GOOGLE_PAY	Servicio móvil de pagos	
Logo	Mastercard	MASTERCARD	Tarjeta de crédito	
Logo	Mastercard	MASTERCARD	Tarjeta de débito	
Logo	Nequi	NEQUI	Servicio móvil de pagos	
Logo	PSE	PSE	Transferencia bancaria	PSE le permite a tus clientes pagar utilizando Nequi y Daviplata.
Logo	Su Red	OTHERS_CASH	Efectivo	Oficinas de pago: PagaTodo, Gana Gana, Gana, Acertemos, Apuestas Cúcuta 75, Su Chance, La Perla, Apuestas Unidas, JER.
Logo	VISA	VISA	Tarjetas de crédito	
Logo	VISA	VISA_DEBIT	Tarjetas de débito	

*/