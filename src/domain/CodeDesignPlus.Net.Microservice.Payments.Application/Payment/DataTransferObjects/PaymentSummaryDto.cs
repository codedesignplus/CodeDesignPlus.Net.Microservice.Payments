using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

/// <summary>
/// Lo que un administrador puede ver de un pago: cuanto, de que, en que estado y por donde entro.
/// </summary>
/// <remarks>
/// It exists because <see cref="PaymentDto"/> cannot be published: it carries the provider's raw answers
/// (<c>InitiateResponse</c> and <c>FinalResponse</c>), whose content the provider decides, so a field the provider
/// adds one day would be published without anyone reviewing it. Of the payment method only its type goes out; the
/// payer chose it on the provider's page and no card data ever reached this service.
/// <para>
/// Cualquier campo que se anada aqui viaja al navegador de quien tenga el permiso de consulta. Antes de
/// anadir uno, mirar <c>PaymentSummaryDtoHasNoSecretsTest</c>: lleva una lista negra de nombres que
/// comprueba por reflexion, y los rechaza aunque vengan anidados.
/// </para>
/// </remarks>
public class PaymentSummaryDto : IDtoBase
{
    /// <summary>Identificador del pago.</summary>
    public required Guid Id { get; set; }

    /// <summary>Modulo que origino el cobro (por ejemplo <c>Invoicing</c> o <c>Parking</c>).</summary>
    public string Module { get; set; } = null!;

    /// <summary>
    /// Identificador del registro del modulo por el que se cobra.
    /// </summary>
    /// <remarks>
    /// Es lo que permite saltar de un cobro a la cuenta de cobro o a la reserva que lo origino. Sin el, el
    /// listado dice que hay un pago de cierto importe pero no de que, que es la mitad de la pregunta.
    /// </remarks>
    public Guid ReferenceId { get; set; }

    /// <summary>Estado en que quedo el pago segun la pasarela.</summary>
    public PaymentStatus Status { get; set; }

    /// <summary>Importe cobrado, en unidades menores y con su moneda.</summary>
    public Net.ValueObjects.Financial.Money Total { get; set; } = null!;

    /// <summary>Descripcion con la que se inicio el cobro.</summary>
    public string Description { get; set; } = null!;

    /// <summary>Code of the provider that processed the transaction.</summary>
    public string PaymentProvider { get; set; } = null!;

    /// <summary>
    /// Instante en que se inicio el cobro.
    /// </summary>
    /// <remarks>
    /// Es la fecha que de verdad se mira en esta pantalla: un pago lleva horas en <c>Initiated</c> o no, y
    /// eso solo se sabe contra el momento en que arranco.
    /// </remarks>
    public Instant CreatedAt { get; set; }

    /// <summary>
    /// Payment method the payer chose on the provider's page (for example <c>CARD</c> or <c>PSE</c>); null until
    /// the provider reports it.
    /// </summary>
    public string? PaymentMethodType { get; set; }

    /// <summary>Nombre del comprador. Nunca su documento.</summary>
    public string BuyerName { get; set; } = null!;
}
