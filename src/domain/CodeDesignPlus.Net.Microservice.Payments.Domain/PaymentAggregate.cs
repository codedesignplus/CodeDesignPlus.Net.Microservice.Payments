using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class PaymentAggregate(Guid id) : AggregateRootBase(id)
{
    public string Module { get; private set; } = null!;
    public Guid? Tenant { get; private set; }

    public PaymentStatus Status { get; private set; }
    public Amount SubTotal { get; private set; } = null!;
    public Amount Tax { get; private set; } = null!;
    public Amount Total { get; private set; } = null!;

    public Payer Payer { get; private set; } = null!;
    public PaymentMethodInfo PaymentMethod { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    public PaymentProvider PaymentProvider { get; private set; } = PaymentProvider.None;
    public string? ProviderTransactionId { get; private set; }
    public string? ProviderResponseMessage { get; private set; }

    public string? RawProviderResponseData { get; private set; }

    private PaymentAggregate(Guid id, string module, Amount subTotal, Amount tax, Amount total, Payer payer, PaymentMethodInfo paymentMethod, string description, PaymentProvider paymentProvider, Guid? tenant, Guid createdBy)
        : this(id)
    {

        DomainGuard.IsNullOrEmpty(module, Errors.ModuleCannotBeNullOrEmpty);
        DomainGuard.IsNull(subTotal, Errors.SubTotalCannotBeNull);
        DomainGuard.IsNull(tax, Errors.TaxCannotBeNull);
        DomainGuard.IsNull(total, Errors.TotalCannotBeNull);
        DomainGuard.IsNull(payer, Errors.BuyerCannotBeNull);
        DomainGuard.IsNull(paymentMethod, Errors.PaymentMethodCannotBeNull);
        DomainGuard.IsNullOrEmpty(description, Errors.DescriptionCannotBeNullOrEmpty);
        DomainGuard.IsNull(paymentProvider, Errors.PaymentProviderCannotBeNull);

        Module = module;
        SubTotal = subTotal;
        Tax = tax;
        Total = total;
        Payer = payer;
        PaymentMethod = paymentMethod;
        Description = description;
        PaymentProvider = paymentProvider;
        CreatedBy = createdBy;
        CreatedAt = SystemClock.Instance.GetCurrentInstant();
        IsActive = true;

        CreatedBy = createdBy;
        CreatedAt = SystemClock.Instance.GetCurrentInstant();
        IsActive = true;

        Status = PaymentStatus.Initiated;

        if (tenant.HasValue)
            Tenant = tenant.Value;

        //base.AddEvent(PaymentCompletedDomainEvent.Create(Id, Provider, Transaction, Request, Response, Tenant));
    }

    public static PaymentAggregate Create(Guid id, 
        string module,
        Amount subTotal,
        Amount tax,
        Amount total,
        Payer payer,
        PaymentMethodInfo paymentMethod,
        string description,
        PaymentProvider paymentProvider,
        Guid? tenant = null,
        Guid createdBy = default)
    {
        return new PaymentAggregate(id, module, subTotal, tax, total, payer, paymentMethod, description, paymentProvider, tenant, createdBy);
    }
    

    /// <summary>
    /// Marca el pago como exitoso después de la confirmación del proveedor.
    /// </summary>
    public void CompleteAsSucceeded(string providerTransactionId, string providerMessage, string rawResponse)
    {
        if (Status != PaymentStatus.Initiated)
            throw new InvalidOperationException("Cannot complete a payment that is not in 'Initiated' state.");
        
        DomainGuard.IsFalse(Status != PaymentStatus.Initiated, Errors.PaymentStatusIsNotInitiated);
        DomainGuard.IsNullOrEmpty(providerTransactionId, Errors.ProviderTransactionIdCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(providerMessage, Errors.ProviderResponseMessageCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(rawResponse, Errors.RawProviderResponseDataCannotBeNullOrEmpty);

        Status = PaymentStatus.Succeeded;
        ProviderTransactionId = providerTransactionId;
        ProviderResponseMessage = providerMessage;
        RawProviderResponseData = rawResponse;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
        
        //AddEvent(new PaymentSucceeded(Id, ReferenceCode, Module, Payer.EmailAddress));
    }

    /// <summary>
    /// Marca el pago como fallido después de la confirmación del proveedor.
    /// </summary>
    public void CompleteAsFailed(string? providerTransactionId, string providerMessage, string rawResponse)
    {
        if (Status != PaymentStatus.Initiated)
            throw new InvalidOperationException("Cannot fail a payment that is not in 'Initiated' state.");
            
        Status = PaymentStatus.Failed;
        ProviderTransactionId = providerTransactionId;
        ProviderResponseMessage = providerMessage;
        RawProviderResponseData = rawResponse;

        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
        
        //AddEvent(new PaymentFailed(Id, ReferenceCode, Module, providerMessage));
    }
}
