using System;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;


/// <summary>
/// DTO de respuesta para la operación InitiatePaymentAsync.
/// Describe qué debe hacer el sistema a continuación.
/// </summary>
public class InitiatePaymentResponseDto
{
    /// <summary>
    /// Indica si la iniciación del pago fue exitosa.
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// El ID del pago en nuestro sistema.
    /// </summary>
    public Guid PaymentId { get; set; }
    /// <summary>
    /// El ID de la transacción proporcionado por el proveedor de pagos.
    /// </summary>
    public string? ProviderTransactionId { get; set; }
    /// <summary>
    /// La siguiente acción que el sistema debe tomar.
    /// </summary>
    public NextActionType NextAction { get; set; }
    /// <summary>
    /// URL de redirección proporcionada por el proveedor de pagos.
    /// </summary>
    public string? RedirectUrl { get; set; }
    /// <summary>
    /// Parámetros adicionales necesarios para integrar el widget de pago del proveedor.
    /// </summary>
    public Dictionary<string, string> WidgetParameters { get; set; } = [];
    /// <summary>
    /// Respuesta cruda del proveedor de pagos para referencia o depuración.
    /// </summary>
    public Dictionary<string, string> ProviderResponse { get; set; } = []; 
}
