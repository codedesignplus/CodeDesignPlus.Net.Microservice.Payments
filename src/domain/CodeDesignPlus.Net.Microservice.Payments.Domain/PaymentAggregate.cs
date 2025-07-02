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
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    public PaymentProvider PaymentProvider { get; private set; } = PaymentProvider.None;
    public string? ProviderTransactionId { get; private set; }
    public string? ProviderResponseMessage { get; private set; }
    public string? RawProviderResponseData { get; private set; }
    public FinancialNetwork FinancialNetwork { get; private set; } = null!;

    private PaymentAggregate(Guid id, string module, Amount subTotal, Amount tax, Amount total, Payer payer, PaymentMethod paymentMethod, string description, PaymentProvider paymentProvider, Guid? tenant, Guid createdBy)
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

        DomainGuard.IsTrue(total.Value < subTotal.Value + tax.Value, Errors.TotalMustBeGreaterThanOrEqualToSubTotalPlusTax);

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

        AddEvent(PaymentInitiatedDomainEvent.Create(Id, Module, SubTotal, Tax, Total, Payer, PaymentMethod, Description, PaymentProvider, Tenant, CreatedBy));
    }

    public static PaymentAggregate Create(Guid id, string module, Amount subTotal, Amount tax, Amount total, Payer payer, PaymentMethod paymentMethod, string description, PaymentProvider paymentProvider, Guid? tenant, Guid createdBy)
    {
        return new PaymentAggregate(id, module, subTotal, tax, total, payer, paymentMethod, description, paymentProvider, tenant, createdBy);
    }

    public void SetResponse(PaymentStatus status, string providerMessage, string rawResponse, FinancialNetwork financialNetwork)
    {
        DomainGuard.IsTrue(Status != PaymentStatus.Initiated, Errors.PaymentStatusIsNotInitiated);
        DomainGuard.IsNull(financialNetwork, Errors.FinancialNetworkCannotBeNull);

        Status = status;
        ProviderResponseMessage = providerMessage;
        RawProviderResponseData = rawResponse;
        FinancialNetwork = financialNetwork;

        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(PaymentResponseAssociatedDomainEvent.Create(Id, Status, ProviderResponseMessage, FinancialNetwork, Tenant));
    }

    public void SetTransactionId(string transactionId)
    {
        DomainGuard.IsNullOrEmpty(transactionId, Errors.TransactionIdCannotBeNullOrEmpty);
        DomainGuard.IsTrue(Status != PaymentStatus.Initiated, Errors.PaymentStatusIsNotInitiated);

        ProviderTransactionId = transactionId;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(TransactionIdAssignedDomainEvent.Create(Id, ProviderTransactionId));
    }
}
