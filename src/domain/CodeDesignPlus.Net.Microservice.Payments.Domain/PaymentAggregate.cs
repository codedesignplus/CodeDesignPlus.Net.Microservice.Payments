using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

/// <summary>
/// A charge asked for by a module and paid on the provider's hosted page.
/// </summary>
/// <remarks>
/// Several processes write the same payment: the provider's webhook, the reconciliation job that asks the provider and
/// closes a page that lapsed with no transaction, the daily sweep of payments with no answer, and the refund events. The
/// job could read a payment as in progress, the webhook approve it in the meantime, and the job then overwrite the
/// approval with «expired» and announce both outcomes to the module (pendings/665, rule 59). Hence
/// <see cref="IVersionedAggregate"/>: each writer rereads and reapplies its change, and declines it when the payment no
/// longer allows it.
/// </remarks>
public class PaymentAggregate(Guid id) : AggregateRootBase(id), IVersionedAggregate
{
    /// <inheritdoc/>
    public long Version { get; private set; }

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
    /// For a card, whether it was credit or debit as the provider said it (for example <c>CREDIT</c> or <c>DEBIT</c>); null
    /// when the provider did not say it or the method is not a card. A loose member (rule 31): the payments stored before
    /// it existed read it as absent (Kappali pendings/661).
    /// </summary>
    public string? CardType { get; private set; }
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
    /// Hasta cuándo acepta el pago la página del proveedor: el mismo instante que se le envió al crearla (pendings/622).
    /// </summary>
    /// <remarks>
    /// Miembro suelto y no argumento del constructor (regla 31): los cobros guardados antes no lo traen y se leen con null.
    /// </remarks>
    public Instant? CheckoutExpiresAt { get; private set; }
    /// <summary>
    /// Why the payment did not succeed, as the provider reported it once normalized (pendings/622).
    /// </summary>
    /// <remarks>
    /// Loose member, not a constructor argument (rule 31): payments closed before it read it as <see cref="PaymentFailureReason.None"/>.
    /// </remarks>
    public PaymentFailureReason FailureReason { get; private set; }
    /// <summary>
    /// What the bank or the provider said about the outcome, as they said it (for example «Fondos insuficientes»), cut to
    /// <see cref="FieldLength.Description"/>. Null when they said nothing or the payment succeeded.
    /// </summary>
    public string? ProviderMessage { get; private set; }
    /// <summary>
    /// Until when the payer can retry on the provider's page after a declined attempt; null while no attempt was declined
    /// (pendings/622).
    /// </summary>
    /// <remarks>
    /// Loose member, not a constructor argument (rule 31): payments stored before it read it as null.
    /// </remarks>
    public Instant? RetryWindowEndsAt { get; private set; }
    /// <summary>
    /// Whether the provider approved the payment after this service had closed it as failed or expired (pendings/622).
    /// </summary>
    /// <remarks>
    /// Loose member, not a constructor argument (rule 31): payments stored before it read it as false.
    /// </remarks>
    public bool ApprovedLate { get; private set; }


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
    /// <param name="response">Lo que respondió la pasarela, con la URL de su página.</param>
    /// <param name="checkoutExpiresAt">El vencimiento que se le envió a la página del proveedor.</param>
    public void SetInitiateResponse(Dictionary<string, string?> response, Instant checkoutExpiresAt)
    {
        DomainGuard.IsTrue(Status != PaymentStatus.Initiated, Errors.OnlyCanSetInitiateResponseIfStatusIsInitiated);
        DomainGuard.IsEmpty(response, Errors.InitiateResponseCannotBeEmpty);

        InitiateResponse = response;
        CheckoutExpiresAt = checkoutExpiresAt;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(PaymentInitiationRespondedDomainEvent.Create(Id, response, Tenant));
    }

    /// <summary>
    /// Records an attempt the provider declined while the payer can still retry on the same hosted page (pendings/622).
    /// </summary>
    /// <remarks>
    /// The payment stays open as <see cref="PaymentStatus.AwaitingRetry"/> until <paramref name="retryWindowEndsAt"/>: a retry
    /// is a new transaction with the same reference, so closing the payment now would leave an approved retry with nowhere to
    /// go. The modules hear about the rejection right away (<see cref="PaymentAttemptDeclinedDomainEvent"/>) and apply its
    /// consequences only with the final outcome, which <see cref="SetFinalResponse"/> records.
    /// <para>
    /// A second declined attempt inside the window changes nothing: the window is the one the first rejection opened, and the
    /// payer was already told.
    /// </para>
    /// </remarks>
    /// <param name="response">What the provider reported about the declined attempt.</param>
    /// <param name="providerTransactionId">Id of the declined transaction at the provider.</param>
    /// <param name="paymentMethodType">Payment method of the declined attempt.</param>
    /// <param name="failureReason">Why it was declined, normalized by the provider adapter.</param>
    /// <param name="providerMessage">What the bank or the provider said, as they said it.</param>
    /// <param name="retryWindowEndsAt">Until when the provider's page lets the payer retry.</param>
    public void DeclineAttempt(Dictionary<string, string?> response, string? providerTransactionId, string? paymentMethodType, PaymentFailureReason failureReason, string? providerMessage, Instant retryWindowEndsAt)
    {
        DomainGuard.IsEmpty(response, Errors.FinalResponseCannotBeEmpty);

        if (Status == PaymentStatus.AwaitingRetry)
        {
            return;
        }

        DomainGuard.IsTrue(Status != PaymentStatus.Initiated && Status != PaymentStatus.Pending, Errors.OnlyCanSetFinalResponseIfStatusIsInitiated);

        Status = PaymentStatus.AwaitingRetry;
        RetryWindowEndsAt = retryWindowEndsAt;
        FinalResponse = response;
        ProviderTransactionId = providerTransactionId;
        PaymentMethodType = paymentMethodType;
        FailureReason = failureReason;
        ProviderMessage = Shorten(providerMessage);
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(PaymentAttemptDeclinedDomainEvent.Create(Id, Module, ReferenceId, Tenant, Provider, PlatformAccount, FailureReason, ProviderMessage, retryWindowEndsAt));
    }

    /// <summary>
    /// Whether the payment waits for a retry whose window is still open at <paramref name="now"/>.
    /// </summary>
    /// <param name="now">The instant to check.</param>
    /// <returns><see langword="true"/> while a declined payment can still be retried on the provider's page.</returns>
    public bool IsAwaitingRetryAt(Instant now) => Status == PaymentStatus.AwaitingRetry && RetryWindowEndsAt is { } endsAt && now < endsAt;

    /// <summary>
    /// Whether the payment has no final outcome yet: started, pending at the provider or waiting for a retry.
    /// </summary>
    public bool IsInProgress => Status is PaymentStatus.Initiated or PaymentStatus.Pending or PaymentStatus.AwaitingRetry;

    /// <summary>
    /// Asocia la respuesta final del proveedor de pagos (recibida vía webhook) y cierra el cobro.
    /// </summary>
    /// <remarks>
    /// Accepts <see cref="PaymentStatus.Expired"/> too: a provider may report that the payer let the session lapse,
    /// and rejecting that outcome would leave the payment in progress forever. It differs from <see cref="Expire"/>,
    /// where nobody answered and the platform closes the payment by silence.
    /// <para>
    /// A payment already closed as failed or expired still takes an approval (pendings/622): a retry approved after its window,
    /// or a PSE confirmed after the page lapsed. The money came in, and rejecting the approval left it charged and applied to
    /// nothing. It is kept as <see cref="ApprovedLate"/> and travels as <c>LateApproval</c>, so the module applies it if what
    /// it paid for still exists and otherwise leaves it as the payer's credit.
    /// </para>
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
    /// <param name="failureReason">Why it did not succeed, normalized by the provider adapter; ignored when it succeeded.</param>
    /// <param name="providerMessage">What the bank or the provider said, as they said it; ignored when it succeeded.</param>
    /// <param name="cardType">For a card, whether it was credit or debit as the provider said it; null when unknown.</param>
    public void SetFinalResponse(PaymentStatus finalStatus, Dictionary<string, string?> response, string? providerTransactionId = null, Money? capturedAmount = null, string? paymentMethodType = null, ProviderFee? fee = null, PaymentFailureReason failureReason = PaymentFailureReason.None, string? providerMessage = null, string? cardType = null)
    {
        DomainGuard.IsTrue(
            finalStatus != PaymentStatus.Succeeded && finalStatus != PaymentStatus.Failed && finalStatus != PaymentStatus.Expired,
            Errors.FinalStatusMustBeResolved);
        DomainGuard.IsEmpty(response, Errors.FinalResponseCannotBeEmpty);

        // Idempotent: if already at the same final status, skip (webhook retry)
        if (Status == finalStatus)
            return;

        var late = finalStatus == PaymentStatus.Succeeded && Status is (PaymentStatus.Failed or PaymentStatus.Expired);

        // Reject conflicting transitions (e.g., Succeeded → Failed); only an approval reopens a closed payment.
        DomainGuard.IsTrue(
            Status is not (PaymentStatus.Initiated or PaymentStatus.Pending or PaymentStatus.AwaitingRetry) && !late,
            Errors.OnlyCanSetFinalResponseIfStatusIsInitiated);

        // A capture in another currency is not this payment: applying it would settle the debt with a wrong amount.
        DomainGuard.IsTrue(capturedAmount is not null && capturedAmount.Currency != Total.Currency, Errors.CapturedAmountCurrencyMustMatchTotal);

        Status = finalStatus;
        ApprovedLate = late;
        FinalResponse = response;
        ProviderTransactionId = providerTransactionId;
        PaymentMethodType = paymentMethodType;
        CardType = string.IsNullOrWhiteSpace(cardType) ? null : cardType.Trim();
        FailureReason = ReasonFor(finalStatus, failureReason);
        ProviderMessage = finalStatus == PaymentStatus.Succeeded ? null : Shorten(providerMessage);

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

        AddEvent(PaymentResponseAssociatedDomainEvent.Create(Id, Module, ReferenceId, Status, FinalResponse, Tenant, CapturedAmount, Provider, ProviderFee, ProviderFeeTax, expectedNet, FeeSource, PlatformAccount, FailureReason, ProviderMessage, ApprovedLate, PaymentMethodType, CardType));
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
    /// <b>Quien lo llama tiene que ser generoso con el plazo.</b> Si la pasarela aprueba despues,
    /// <see cref="SetFinalResponse"/> lo acepta como aprobacion tardia (pendings/622), pero lo que el cobro esperaba ya se
    /// cancelo y el dinero queda como saldo a favor del pagador. Esperar de mas solo alarga una cotizacion congelada;
    /// cerrar de menos convierte un pago normal en uno tardio.
    /// </para>
    /// </remarks>
    /// <param name="reason">Por que se cierra, que viaja en la respuesta para que quede dicho en el registro.</param>
    public void Expire(string reason)
    {
        DomainGuard.IsTrue(
            Status is not (PaymentStatus.Initiated or PaymentStatus.Pending or PaymentStatus.AwaitingRetry),
            Errors.OnlyPaymentsInProgressCanExpire);

        Status = PaymentStatus.Expired;
        FailureReason = PaymentFailureReason.Expired;
        ProviderMessage = null;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        // Se deja constancia de que esto lo decidio la plataforma y no el proveedor. La clave dice quien
        // habla: quien lea el registro manana tiene que poder distinguir un rechazo del banco de un cierre
        // nuestro por silencio.
        FinalResponse = new Dictionary<string, string?>
        {
            ["expiredBy"] = "platform",
            ["reason"] = reason,
        };

        AddEvent(PaymentResponseAssociatedDomainEvent.Create(Id, Module, ReferenceId, Status, FinalResponse, Tenant, null, Provider, platformAccount: PlatformAccount, failureReason: FailureReason));
    }

    /// <summary>
    /// Whether a user, in the tenant of the session, can read this payment: it belongs to that tenant or the user started it.
    /// </summary>
    /// <remarks>
    /// The one who started it can read it whatever the session's tenant, because a platform payment (a license) is read by
    /// the administrator who buys it, possibly before the tenant it pays for exists (same criterion as
    /// <c>GET Order/{id}</c> in ms-licenses, pendings/735).
    /// </remarks>
    /// <param name="tenant">The tenant of the session.</param>
    /// <param name="user">The user of the session.</param>
    /// <returns><see langword="true"/> if the payment belongs to the tenant or the user started it.</returns>
    public bool IsVisibleTo(Guid tenant, Guid user) => (tenant != Guid.Empty && Tenant == tenant) || (user != Guid.Empty && CreatedBy == user);

    /// <summary>
    /// The reason a closed payment keeps: none when it succeeded, <see cref="PaymentFailureReason.Expired"/> when its session
    /// lapsed, and what the provider reported when it failed.
    /// </summary>
    private static PaymentFailureReason ReasonFor(PaymentStatus status, PaymentFailureReason reported) => status switch
    {
        PaymentStatus.Succeeded => PaymentFailureReason.None,
        PaymentStatus.Expired => PaymentFailureReason.Expired,
        _ => reported
    };

    /// <summary>
    /// The provider's message without surrounding blanks and cut to the standard length of a description (rule 48).
    /// </summary>
    private static string? Shorten(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var trimmed = message.Trim();

        return trimmed.Length <= FieldLength.Description ? trimmed : trimmed[..FieldLength.Description];
    }
}
