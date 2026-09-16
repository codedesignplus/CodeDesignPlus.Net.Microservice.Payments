using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

/// <summary>
/// Lo que un administrador puede ver de un pago: cuanto, de que, en que estado y por donde entro.
/// </summary>
/// <remarks>
/// Existe porque <see cref="PaymentDto"/> no se puede publicar. Aquel expone el objeto
/// <c>PaymentMethod</c> entero —token de la pasarela, titular de la tarjeta y codigo de seguridad— y el
/// <c>Buyer</c> completo con su numero de documento. Mientras nadie lo devolviera por REST no habia fuga;
/// el dia que hiciera falta una pantalla de consulta, el camino corto era exponerlo tal cual.
/// <para>
/// De la tarjeta solo salen el <b>tipo</b> de medio y los cuatro ultimos digitos, que es lo justo para que
/// alguien reconozca su propio pago. Y <c>InitiateResponse</c> y <c>FinalResponse</c> se quedan fuera
/// aunque parezcan inocentes: son la respuesta cruda del proveedor, su contenido lo decide el proveedor y
/// no nosotros, y basta con que un dia incluya un campo nuevo para que se publique sin que nadie lo revise.
/// </para>
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

    /// <summary>Pasarela que proceso la transaccion.</summary>
    public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.None;

    /// <summary>
    /// Instante en que se inicio el cobro.
    /// </summary>
    /// <remarks>
    /// Es la fecha que de verdad se mira en esta pantalla: un pago lleva horas en <c>Initiated</c> o no, y
    /// eso solo se sabe contra el momento en que arranco.
    /// </remarks>
    public Instant CreatedAt { get; set; }

    /// <summary>
    /// Tipo de medio de pago tal cual lo normaliza el objeto de valor: <c>VISA</c>, <c>PSE</c>, etc.
    /// </summary>
    public string PaymentMethodType { get; set; } = null!;

    /// <summary>Nombre del comprador. Nunca su documento.</summary>
    public string BuyerName { get; set; } = null!;

    /// <summary>
    /// Los cuatro ultimos digitos de la tarjeta, o nulo si el medio no es tarjeta.
    /// </summary>
    /// <remarks>
    /// Es el maximo que se puede mostrar de un numero de tarjeta, y esta aqui solo para desempatar entre
    /// dos cobros del mismo dia por el mismo importe.
    /// </remarks>
    public string? Last4Digits { get; set; }
}
