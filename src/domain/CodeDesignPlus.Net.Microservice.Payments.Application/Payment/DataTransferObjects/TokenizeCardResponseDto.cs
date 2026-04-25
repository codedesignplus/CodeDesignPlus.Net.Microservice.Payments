namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

/// <summary>
/// DTO de respuesta estandarizado para la operación de tokenización de tarjeta.
/// </summary>
public class TokenizeCardResponseDto
{
    /// <summary>
    /// Indica si la tokenización fue exitosa.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// El token generado por el proveedor. Debe almacenarse para usarse en pagos futuros.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// El número de tarjeta enmascarado seguro para mostrar al usuario (ej. "403799******1984").
    /// </summary>
    public string? MaskedNumber { get; set; }

    /// <summary>
    /// La franquicia o método de pago (ej. "VISA", "MASTERCARD").
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Descripción del error en caso de que la tokenización haya fallado.
    /// </summary>
    public string? Error { get; set; }
}
