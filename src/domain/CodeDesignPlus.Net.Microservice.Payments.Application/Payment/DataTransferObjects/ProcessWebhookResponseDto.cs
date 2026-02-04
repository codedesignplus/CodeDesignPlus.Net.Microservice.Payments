using System;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;


/// <summary>
/// DTO de respuesta para la operación ProcessWebhookAsync.
/// Contiene la información validada y estandarizada del webhook.
/// </summary>
public class ProcessWebhookResponseDto
{
    /// <summary>
    /// Indica si la firma del webhook es válida.
    /// </summary>
    public bool IsSignatureValid { get; set; }
    /// <summary>
    /// El ID del pago en nuestro sistema.
    /// </summary>
    public Guid PaymentId { get; set; } 
    /// <summary>
    /// El estado final de la transacción de pago.
    /// </summary>
    public PaymentStatus FinalStatus { get; set; }
    /// <summary>
    /// Datos crudos validados del webhook.
    /// </summary>
    public Dictionary<string, string> RawData { get; set; } = [];
}