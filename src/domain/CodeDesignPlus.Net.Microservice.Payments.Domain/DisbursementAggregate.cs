using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class DisbursementAggregate(Guid id) : AggregateRootBase(id)
{
    public Guid PaymentId { get; private set; }
    public Guid BeneficiaryId { get; private set; }
    public Guid BeneficiaryUserId { get; private set; }
    public long TotalAmount { get; private set; }
    public long CommissionAmount { get; private set; }
    public long DisbursedAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public DisbursementStatus Status { get; private set; }
    public Instant? ProcessedAt { get; private set; }
    public string? ProviderReference { get; private set; }
    public string? FailureReason { get; private set; }
    public Guid Tenant { get; private set; }

    public static DisbursementAggregate Create(
        Guid id,
        Guid paymentId,
        Guid beneficiaryId,
        Guid beneficiaryUserId,
        long totalAmount,
        long commissionAmount,
        long disbursedAmount,
        string currency,
        Guid tenant)
    {
        DomainGuard.GuidIsEmpty(id, Errors.PaymentIdCannotBeEmpty);
        DomainGuard.GuidIsEmpty(paymentId, Errors.DisbursementPaymentIdIsRequired);
        DomainGuard.GuidIsEmpty(beneficiaryId, Errors.DisbursementBeneficiaryIdIsRequired);
        DomainGuard.GuidIsEmpty(beneficiaryUserId, Errors.DisbursementBeneficiaryUserIdIsRequired);
        DomainGuard.IsTrue(totalAmount <= 0, Errors.DisbursementTotalAmountMustBePositive);
        DomainGuard.IsTrue(disbursedAmount <= 0, Errors.DisbursementAmountMustBePositive);
        DomainGuard.IsNullOrEmpty(currency, Errors.CurrencyIsRequired);

        var aggregate = new DisbursementAggregate(id)
        {
            PaymentId = paymentId,
            BeneficiaryId = beneficiaryId,
            BeneficiaryUserId = beneficiaryUserId,
            TotalAmount = totalAmount,
            CommissionAmount = commissionAmount,
            DisbursedAmount = disbursedAmount,
            Currency = currency,
            Status = DisbursementStatus.Pending,
            Tenant = tenant,
            IsActive = true,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        aggregate.AddEvent(DisbursementCreatedDomainEvent.Create(id, paymentId, beneficiaryId, disbursedAmount, currency, tenant));

        return aggregate;
    }

    public void MarkAsProcessing()
    {
        DomainGuard.IsTrue(Status != DisbursementStatus.Pending, Errors.DisbursementNotPending);

        Status = DisbursementStatus.Processing;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public void Complete(string providerReference)
    {
        DomainGuard.IsTrue(Status != DisbursementStatus.Processing, Errors.DisbursementNotProcessing);
        DomainGuard.IsNullOrEmpty(providerReference, Errors.ProviderReferenceIsRequired);

        Status = DisbursementStatus.Completed;
        ProviderReference = providerReference;
        ProcessedAt = SystemClock.Instance.GetCurrentInstant();
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(DisbursementCompletedDomainEvent.Create(Id, BeneficiaryId, DisbursedAmount, providerReference, Tenant));
    }

    public void Fail(string reason)
    {
        DomainGuard.IsTrue(Status != DisbursementStatus.Processing, Errors.DisbursementNotProcessing);
        DomainGuard.IsNullOrEmpty(reason, Errors.FailureReasonIsRequired);

        Status = DisbursementStatus.Failed;
        FailureReason = reason;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }
}
