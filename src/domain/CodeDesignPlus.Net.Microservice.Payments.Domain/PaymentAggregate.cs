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

    public void CompleteAsSucceeded(string providerTransactionId, string providerMessage, string rawResponse)
    {
        DomainGuard.IsFalse(Status != PaymentStatus.Initiated, Errors.PaymentStatusIsNotInitiated);
        DomainGuard.IsNullOrEmpty(providerTransactionId, Errors.ProviderTransactionIdCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(providerMessage, Errors.ProviderResponseMessageCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(rawResponse, Errors.RawProviderResponseDataCannotBeNullOrEmpty);

        Status = PaymentStatus.Succeeded;
        ProviderTransactionId = providerTransactionId;
        ProviderResponseMessage = providerMessage;
        RawProviderResponseData = rawResponse;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(PaymentSucceededDomainEvent.Create(Id, ProviderTransactionId, ProviderResponseMessage, Payer, Tenant));
    }

    public void CompleteAsFailed(string providerTransactionId, string providerMessage, string rawResponse)
    {
        DomainGuard.IsFalse(Status != PaymentStatus.Initiated, Errors.PaymentStatusIsNotInitiated);
        DomainGuard.IsNullOrEmpty(providerTransactionId, Errors.ProviderTransactionIdCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(providerMessage, Errors.ProviderResponseMessageCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(rawResponse, Errors.RawProviderResponseDataCannotBeNullOrEmpty);

        Status = PaymentStatus.Failed;
        ProviderTransactionId = providerTransactionId;
        ProviderResponseMessage = providerMessage;
        RawProviderResponseData = rawResponse;

        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(PaymentFailedDomainEvent.Create(Id, ProviderTransactionId, ProviderResponseMessage, Payer, Tenant));
    }
}
