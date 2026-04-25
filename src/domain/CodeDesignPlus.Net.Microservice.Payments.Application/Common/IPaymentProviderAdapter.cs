using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Common;


/// <summary>
/// Define un contrato genérico para cualquier proveedor de pagos.
/// Las operaciones hablan en términos de los agregados del dominio y devuelven DTOs estandarizados.
/// </summary>
public interface IPaymentProviderAdapter
{
    /// <summary>
    /// El proveedor que esta implementación representa (ej. PayU, Stripe).
    /// </summary>
    PaymentProvider Provider { get; }
    
    /// <summary>
    /// Inicia una transacción de pago con el proveedor.
    /// </summary>
    /// <param name="payment">El agregado de pago que contiene toda la información de la transacción.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Un DTO estandarizado que describe la acción que el sistema debe tomar a continuación.</returns>
    Task<InitiatePaymentResponseDto> InitiatePaymentAsync(PaymentAggregate payment, CancellationToken cancellationToken);

    /// <summary>
    /// Procesa y valida una notificación entrante (webhook) del proveedor de pagos.
    /// </summary>
    /// <param name="request">La solicitud HTTP entrante que contiene los datos del webhook.</param>
    /// <returns>Un DTO estandarizado con el resultado de la transacción (ej. Succeeded, Failed) y los datos validados.</returns>
    Task<ProcessWebhookResponseDto> ProcessWebhookAsync(HttpRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Tokeniza una tarjeta de crédito con el proveedor de pagos para uso futuro sin exponer el número de tarjeta.
    /// </summary>
    /// <param name="name">Nombre del titular de la tarjeta.</param>
    /// <param name="identificationNumber">Número de identificación del titular.</param>
    /// <param name="paymentMethod">Franquicia de la tarjeta (ej. VISA, MASTERCARD).</param>
    /// <param name="cardNumber">Número completo de la tarjeta.</param>
    /// <param name="expirationDate">Fecha de vencimiento en formato YYYY/MM.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Un DTO estandarizado con el token generado y los datos enmascarados de la tarjeta.</returns>
    Task<TokenizeCardResponseDto> TokenizeCardAsync(string name, string identificationNumber, string paymentMethod, string cardNumber, string expirationDate, CancellationToken cancellationToken);
}