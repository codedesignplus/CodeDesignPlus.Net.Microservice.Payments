using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class PaymentAggregate(Guid id) : AggregateRootBase(id)
{
    /// <summary>
    /// El módulo con el que ms-licenses cobra la venta de una licencia.
    /// </summary>
    /// <remarks>
    /// Esos pagos son registro de la plataforma y no de la copropiedad: se conservan al purgarla aunque lleven su
    /// <see cref="Tenant"/>, que sale del <c>X-Tenant</c> de quien compra (plan 078).
    /// </remarks>
    public const string LicensesModule = "Licenses";

    /// <summary>
    /// Módulo o contexto que genera el pago (ej. "Orders", "Subscriptions").
    /// </summary>
    public string Module { get; private set; } = null!;
    /// <summary>
    /// Identificador de referencia del modulo que invoca el pago.
    /// </summary>
    public Guid ReferenceId { get; private set; }
    /// <summary>
    /// Identificador del tenant al que pertenece el pago.
    /// </summary>
    public Guid? Tenant { get; private set; }
    /// <summary>
    /// Estado actual del pago.
    /// </summary>
    public PaymentStatus Status { get; private set; }
    /// <summary>
    /// Subtotal del pago.
    /// </summary>
    public Money SubTotal { get; private set; } = null!;
    /// <summary>
    /// Impuestos aplicados al pago.
    /// </summary>
    public Money Tax { get; private set; } = null!;
    /// <summary>
    /// Total del pago (Subtotal + Impuestos).
    /// </summary>
    public Money Total { get; private set; } = null!;
    /// <summary>
    /// Información del comprador.
    /// </summary>
    public Net.ValueObjects.User.Buyer Buyer { get; private set; } = null!;
    /// <summary>
    /// Descripción del pago.
    /// </summary>
    public string Description { get; private set; } = null!;
    /// <summary>
    /// Code of the provider that processes the payment (for example "Wompi").
    /// </summary>
    public string Provider { get; private set; } = null!;
    /// <summary>
    /// Whether the payment is charged to the platform's own merchant account instead of the tenant's.
    /// </summary>
    public bool PlatformAccount { get; private set; }
    /// <summary>
    /// Id of the transaction at the provider, known once the provider reports it.
    /// </summary>
    public string? ProviderTransactionId { get; private set; }
    /// <summary>
    /// What the provider actually captured. Only set once the payment succeeded.
    /// </summary>
    public Money? CapturedAmount { get; private set; }
    /// <summary>
    /// Payment method the payer chose on the provider's hosted page (for example "CARD" or "PSE").
    /// </summary>
    public string? PaymentMethodType { get; private set; }
    /// <summary>
    /// The commission the provider keeps from this payment, without its tax. Only set once the payment succeeded and
    /// the commission could be known.
    /// </summary>
    public Money? ProviderFee { get; private set; }
    /// <summary>
    /// The tax on <see cref="ProviderFee"/>.
    /// </summary>
    public Money? ProviderFeeTax { get; private set; }
    /// <summary>
    /// Where the commission came from (<see cref="ProviderFeeSources"/>).
    /// </summary>
    public string? FeeSource { get; private set; }
    /// <summary>
    /// The refunds of this payment, as the provider last reported each one; null while there are none.
    /// </summary>
    public List<PaymentRefund>? Refunds { get; private set; }
    /// <summary>
    /// Almacena la respuesta inmediata del proveedor de pagos al iniciar la transacción.
    /// Puede contener una URL de redirección o datos para un widget.
    /// </summary>
    public Dictionary<string, string?> InitiateResponse { get; private set; } = [];
    /// <summary>
    /// Almacena la respuesta final y asíncrona del proveedor (ej. del webhook).
    /// </summary>
    public Dictionary<string, string?> FinalResponse { get; private set; } = [];


    /// <summary>
    /// Crea una nueva instancia de PaymentAggregate.
    /// </summary>
    /// <param name="id">Identificador único del pago.</param>
    /// <param name="module">Módulo o contexto que genera el pago.</param>
    /// <param name="referenceId">Identificador de referencia del modulo que invoca el pago.</param>
    /// <param name="subTotal">Subtotal del pago.</param>
    /// <param name="tax">Impuestos aplicados al pago.</param>
    /// <param name="total">Total del pago (Subtotal + Impuestos).</param>
    /// <param name="buyer">Who pays; prefills the provider's hosted page.</param>
    /// <param name="description">Descripción del pago.</param>
    /// <param name="provider">Code of the provider that processes the payment.</param>
    /// <param name="platformAccount">Whether the payment goes to the platform's own merchant account.</param>
    /// <param name="tenant">Identificador del tenant al que pertenece el pago.</param>
    /// <param name="createdBy">Identificador del usuario que creó el pago.</param>
    /// <returns>Nueva instancia de PaymentAggregate.</returns>
    public static PaymentAggregate Create(Guid id, string module, Guid referenceId, Money subTotal, Money tax, Money total, Net.ValueObjects.User.Buyer buyer, string description, string provider, bool platformAccount, Guid? tenant, Guid createdBy)
    {
        DomainGuard.GuidIsEmpty(id, Errors.PaymentIdCannotBeEmpty);
        DomainGuard.IsNullOrEmpty(module, Errors.ModuleCannotBeNullOrEmpty);
        DomainGuard.GuidIsEmpty(referenceId, Errors.ReferenceIdCannotBeEmpty);
        DomainGuard.IsNull(subTotal, Errors.SubTotalCannotBeNull);
        DomainGuard.IsNull(tax, Errors.TaxCannotBeNull);
        DomainGuard.IsNull(total, Errors.TotalCannotBeNull);
        DomainGuard.IsNull(buyer, Errors.BuyerCannotBeNull);
        DomainGuard.IsNullOrEmpty(description, Errors.DescriptionCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(provider, Errors.PaymentProviderIsRequired);
        
        DomainGuard.IsTrue(total != (subTotal + tax), Errors.TotalMustBeEqualToSubTotalPlusTax);

        var aggregate = new PaymentAggregate(id)
        {
            Module = module,
            ReferenceId = referenceId,
            SubTotal = subTotal,
            Tax = tax,
            Total = total,
            Buyer = buyer,
            Description = description,
            Provider = provider,
            PlatformAccount = platformAccount,
            CreatedBy = createdBy,
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            IsActive = true,

            Status = PaymentStatus.Initiated
        };

        // Un tenant vacío no es una copropiedad. Qué pagos muestra cada pantalla lo decide quien consulta (pendings/295).
        if (tenant.HasValue && tenant.Value != Guid.Empty)
            aggregate.Tenant = tenant.Value;

        aggregate.AddEvent(PaymentInitiatedDomainEvent.Create(id, module, subTotal, tax, total, buyer, description, provider, aggregate.Tenant));

        return aggregate;
    }

    /// <summary>
    /// Asocia la respuesta inmediata de la pasarela de pagos después de iniciar la transacción.
    /// </summary>
    public void SetInitiateResponse(Dictionary<string, string?> response)
    {
        DomainGuard.IsTrue(Status != PaymentStatus.Initiated, Errors.OnlyCanSetInitiateResponseIfStatusIsInitiated);
        DomainGuard.IsEmpty(response, Errors.InitiateResponseCannotBeEmpty);

        InitiateResponse = response;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(PaymentInitiationRespondedDomainEvent.Create(Id, response, Tenant));
    }

    /// <summary>
    /// Asocia la respuesta final del proveedor de pagos (recibida vía webhook) y cierra el cobro.
    /// </summary>
    /// <remarks>
    /// Accepts <see cref="PaymentStatus.Expired"/> too: a provider may report that the payer let the session lapse,
    /// and rejecting that outcome would leave the payment in progress forever. It differs from <see cref="Expire"/>,
    /// where nobody answered and the platform closes the payment by silence.
    /// </remarks>
    /// <param name="finalStatus">The resolved status reported by the provider.</param>
    /// <param name="response">What the provider reported, kept as the record of the outcome.</param>
    /// <param name="providerTransactionId">Id of the transaction at the provider.</param>
    /// <param name="capturedAmount">What the provider captured; only kept when the payment succeeded.</param>
    /// <param name="paymentMethodType">Payment method the payer chose on the provider's page.</param>
    /// <param name="fee">
    /// The commission the provider keeps from the captured amount; only kept when the payment succeeded and it is in the
    /// currency of the payment.
    /// </param>
    public void SetFinalResponse(PaymentStatus finalStatus, Dictionary<string, string?> response, string? providerTransactionId = null, Money? capturedAmount = null, string? paymentMethodType = null, ProviderFee? fee = null)
    {
        DomainGuard.IsTrue(
            finalStatus != PaymentStatus.Succeeded && finalStatus != PaymentStatus.Failed && finalStatus != PaymentStatus.Expired,
            Errors.FinalStatusMustBeResolved);
        DomainGuard.IsEmpty(response, Errors.FinalResponseCannotBeEmpty);

        // Idempotent: if already at the same final status, skip (webhook retry)
        if (Status == finalStatus)
            return;

        // Reject conflicting transitions (e.g., Succeeded → Failed)
        DomainGuard.IsTrue(
            Status != PaymentStatus.Initiated && Status != finalStatus,
            Errors.OnlyCanSetFinalResponseIfStatusIsInitiated);

        // A capture in another currency is not this payment: applying it would settle the debt with a wrong amount.
        DomainGuard.IsTrue(capturedAmount is not null && capturedAmount.Currency != Total.Currency, Errors.CapturedAmountCurrencyMustMatchTotal);

        Status = finalStatus;
        FinalResponse = response;
        ProviderTransactionId = providerTransactionId;
        PaymentMethodType = paymentMethodType;

        // Only a succeeded payment captured money; a declined one may still report the amount it tried to charge.
        CapturedAmount = finalStatus == PaymentStatus.Succeeded ? capturedAmount : null;

        // A commission is only known of money that came in, and only in the currency it came in.
        var keepsFee = CapturedAmount is not null && fee is not null
            && fee.Fee.Currency == CapturedAmount.Currency && fee.Tax.Currency == CapturedAmount.Currency;

        ProviderFee = keepsFee ? fee!.Fee : null;
        ProviderFeeTax = keepsFee ? fee!.Tax : null;
        FeeSource = keepsFee ? fee!.Source : null;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        var expectedNet = keepsFee ? fee!.NetOf(CapturedAmount!) : null;

        AddEvent(PaymentResponseAssociatedDomainEvent.Create(Id, Module, ReferenceId, Status, FinalResponse, Tenant, CapturedAmount, Provider, ProviderFee, ProviderFeeTax, expectedNet, FeeSource, PlatformAccount));
    }

    /// <summary>
    /// Records what the provider reported about a refund of this payment.
    /// </summary>
    /// <remarks>
    /// A redelivered report changes nothing, and a refund that reached a final status keeps it: the provider says those
    /// never change, so a late «pending» cannot reopen one that was already approved.
    /// </remarks>
    /// <param name="refundId">Id of the refund at the provider.</param>
    /// <param name="status">Its normalized status.</param>
    /// <param name="amount">What is refunded, when the provider says it.</param>
    /// <param name="code">Code of the provider's reason, when it did not go through.</param>
    /// <param name="message">The provider's reason, when it did not go through.</param>
    /// <returns>Whether the payment changed and must be saved.</returns>
    public bool RegisterRefund(string refundId, RefundStatus status, Money? amount, string? code, string? message)
    {
        DomainGuard.IsNullOrEmpty(refundId, Errors.RefundIdIsRequired);
        DomainGuard.IsTrue(Status != PaymentStatus.Succeeded, Errors.OnlySucceededPaymentsCanBeRefunded);

        Refunds ??= [];

        var current = Refunds.Find(x => x.RefundId == refundId);

        if (current is not null && (current.Status == status || current.IsFinal))
        {
            return false;
        }

        var now = SystemClock.Instance.GetCurrentInstant();
        var refund = new PaymentRefund(refundId, status, amount ?? current?.Amount, code, message, now);

        if (current is not null)
        {
            Refunds.Remove(current);
        }

        Refunds.Add(refund);
        UpdatedAt = now;

        AddEvent(PaymentRefundUpdatedDomainEvent.Create(Id, Module, ReferenceId, Tenant, Provider, ProviderTransactionId, refund));

        return true;
    }

    /// <summary>
    /// Cierra un cobro que lleva demasiado tiempo sin respuesta de la pasarela.
    /// </summary>
    /// <remarks>
    /// Un cobro que nadie termina se queda en <see cref="PaymentStatus.Initiated"/> para siempre, y eso no es
    /// inofensivo: el modulo que lo pidio sigue esperandolo. En facturacion, la cotizacion que congela el
    /// importe a cobrar queda viva indefinidamente, y con ella el saldo del propietario retenido.
    /// <para>
    /// <b>Va por su propio metodo y no por <see cref="SetFinalResponse"/>.</b> Aquel exige una respuesta del
    /// proveedor, y aqui no la hay: nadie contesto, que es justo el problema. Meter un
    /// <c>Dictionary</c> inventado para poder reutilizarlo seria escribir en los libros una respuesta que la
    /// pasarela nunca dio.
    /// </para>
    /// <para>
    /// <b>Levanta el mismo evento que un rechazo</b>, y eso es el punto entero: el consumidor de facturacion
    /// ya trata <c>Expired</c> igual que <c>Failed</c> y retira la cotizacion de ese pago. Un solo cierre
    /// resuelve las dos limpiezas, sin inventar un segundo barrido que hurgue en el estado de otro servicio.
    /// </para>
    /// <para>
    /// <b>Es irreversible, y por eso quien lo llama tiene que ser generoso con el plazo.</b> Si la pasarela
    /// contesta despues, <see cref="SetFinalResponse"/> rechaza la transicion y el aviso acaba en la cola de
    /// errores: el dinero se cobro y no se aplico. Esperar de mas solo alarga una cotizacion congelada;
    /// cerrar de menos pierde un cobro real.
    /// </para>
    /// </remarks>
    /// <param name="reason">Por que se cierra, que viaja en la respuesta para que quede dicho en el registro.</param>
    public void Expire(string reason)
    {
        DomainGuard.IsTrue(
            Status != PaymentStatus.Initiated && Status != PaymentStatus.Pending,
            Errors.OnlyPaymentsInProgressCanExpire);

        Status = PaymentStatus.Expired;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        // Se deja constancia de que esto lo decidio la plataforma y no el proveedor. La clave dice quien
        // habla: quien lea el registro manana tiene que poder distinguir un rechazo del banco de un cierre
        // nuestro por silencio.
        FinalResponse = new Dictionary<string, string?>
        {
            ["expiredBy"] = "platform",
            ["reason"] = reason,
        };

        AddEvent(PaymentResponseAssociatedDomainEvent.Create(Id, Module, ReferenceId, Status, FinalResponse, Tenant, null, Provider, platformAccount: PlatformAccount));
    }
}
