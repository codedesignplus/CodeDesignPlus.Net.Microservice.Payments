using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class PaymentAggregate(Guid id) : AggregateRootBase(id)
{
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
    public Amount SubTotal { get; private set; } = null!;
    /// <summary>
    /// Impuestos aplicados al pago.
    /// </summary>
    public Amount Tax { get; private set; } = null!;
    /// <summary>
    /// Total del pago (Subtotal + Impuestos).
    /// </summary>
    public Amount Total { get; private set; } = null!;
    /// <summary>
    /// Información del pagador.
    /// </summary>
    public Payer Payer { get; private set; } = null!;
    /// <summary>
    /// Método de pago seleccionado por el pagador.
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    /// <summary>
    /// Descripción del pago.
    /// </summary>
    public string Description { get; private set; } = null!;
    /// <summary>
    /// Proveedor de pagos seleccionado para procesar la transacción.
    /// </summary>
    public PaymentProvider PaymentProvider { get; private set; } = PaymentProvider.None;
    /// <summary>
    /// Almacena la respuesta inmediata del proveedor de pagos al iniciar la transacción.
    /// Puede contener una URL de redirección o datos para un widget.
    /// </summary>
    public Dictionary<string, string?> InitiateResponse { get; private set; } = [];
    /// <summary>
    /// Almacena la respuesta final y asíncrona del proveedor (ej. del webhook).
    /// </summary>
    public Dictionary<string, string> FinalResponse { get; private set; } = [];


    /// <summary>
    /// Crea una nueva instancia de PaymentAggregate.
    /// </summary>
    /// <param name="id">Identificador único del pago.</param>
    /// <param name="module">Módulo o contexto que genera el pago.</param>
    /// <param name="referenceId">Identificador de referencia del modulo que invoca el pago.</param>
    /// <param name="subTotal">Subtotal del pago.</param>
    /// <param name="tax">Impuestos aplicados al pago.</param>
    /// <param name="total">Total del pago (Subtotal + Impuestos).</param>
    /// <param name="payer">Información del pagador.</param>
    /// <param name="paymentMethod">Método de pago seleccionado por el pagador.</param>
    /// <param name="description">Descripción del pago.</param>
    /// <param name="paymentProvider">Proveedor de pagos seleccionado para procesar la transacción.</param>
    /// <param name="tenant">Identificador del tenant al que pertenece el pago.</param>
    /// <param name="createdBy">Identificador del usuario que creó el pago.</param>
    /// <returns>Nueva instancia de PaymentAggregate.</returns>
    public static PaymentAggregate Create(Guid id, string module, Guid referenceId, Amount subTotal, Amount tax, Amount total, Payer payer, PaymentMethod paymentMethod, string description, PaymentProvider paymentProvider, Guid? tenant, Guid createdBy)
    {
        DomainGuard.GuidIsEmpty(id, Errors.PaymentIdCannotBeEmpty);
        DomainGuard.IsNullOrEmpty(module, Errors.ModuleCannotBeNullOrEmpty);
        DomainGuard.GuidIsEmpty(referenceId, Errors.ReferenceIdCannotBeEmpty);
        DomainGuard.IsNull(subTotal, Errors.SubTotalCannotBeNull);
        DomainGuard.IsNull(tax, Errors.TaxCannotBeNull);
        DomainGuard.IsNull(total, Errors.TotalCannotBeNull);
        DomainGuard.IsNull(payer, Errors.BuyerCannotBeNull);
        DomainGuard.IsNull(paymentMethod, Errors.PaymentMethodCannotBeNull);
        DomainGuard.IsNullOrEmpty(description, Errors.DescriptionCannotBeNullOrEmpty);
        DomainGuard.IsNull(paymentProvider, Errors.PaymentProviderCannotBeNull);

        DomainGuard.IsTrue(total.Value < subTotal.Value + tax.Value, Errors.TotalMustBeGreaterThanOrEqualToSubTotalPlusTax);

        var aggregate = new PaymentAggregate(id)
        {
            Module = module,
            ReferenceId = referenceId,
            SubTotal = subTotal,
            Tax = tax,
            Total = total,
            Payer = payer,
            PaymentMethod = paymentMethod,
            Description = description,
            PaymentProvider = paymentProvider,
            CreatedBy = createdBy,
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            IsActive = true,

            Status = PaymentStatus.Initiated
        };

        if (tenant.HasValue)
            aggregate.Tenant = tenant.Value;

        aggregate.AddEvent(PaymentInitiatedDomainEvent.Create(id, module, subTotal, tax, total, payer, paymentMethod, description, paymentProvider, tenant, createdBy));

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
    /// Asocia la respuesta final del proveedor de pagos (recibida vía webhook)
    /// y actualiza el estado del pago a 'Succeeded' o 'Failed'.
    /// </summary>
    public void SetFinalResponse(PaymentStatus finalStatus, Dictionary<string, string> response)
    {
        DomainGuard.IsTrue(Status != PaymentStatus.Initiated, Errors.OnlyCanSetFinalResponseIfStatusIsInitiated);
        DomainGuard.IsTrue(finalStatus != PaymentStatus.Succeeded && finalStatus != PaymentStatus.Failed, Errors.FinalStatusMustBeSucceededOrFailed);
        DomainGuard.IsEmpty(response, Errors.FinalResponseCannotBeEmpty);

        Status = finalStatus;
        FinalResponse = response;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(PaymentResponseAssociatedDomainEvent.Create(Id, Module, ReferenceId, Status, FinalResponse, Tenant));
    }

}
