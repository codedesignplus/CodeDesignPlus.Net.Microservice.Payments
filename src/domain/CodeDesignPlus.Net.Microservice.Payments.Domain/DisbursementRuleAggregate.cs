using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class DisbursementRuleAggregate(Guid id) : AggregateRootBase(id)
{
    public CommissionType CommissionType { get; private set; }
    public long CommissionAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid Tenant { get; private set; }

    public static DisbursementRuleAggregate Create(
        Guid id,
        CommissionType commissionType,
        long commissionAmount,
        string currency,
        string? description,
        Guid tenant)
    {
        DomainGuard.GuidIsEmpty(id, Errors.PaymentIdCannotBeEmpty);
        DomainGuard.IsTrue(commissionAmount <= 0, Errors.CommissionAmountMustBePositive);
        DomainGuard.IsNullOrEmpty(currency, Errors.DisbursementRuleCurrencyIsRequired);

        var aggregate = new DisbursementRuleAggregate(id)
        {
            CommissionType = commissionType,
            CommissionAmount = commissionAmount,
            Currency = currency,
            Description = description,
            Tenant = tenant,
            IsActive = true,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        return aggregate;
    }

    public void Update(
        CommissionType commissionType,
        long commissionAmount,
        string currency,
        string? description)
    {
        DomainGuard.IsTrue(commissionAmount <= 0, Errors.CommissionAmountMustBePositive);
        DomainGuard.IsNullOrEmpty(currency, Errors.DisbursementRuleCurrencyIsRequired);

        CommissionType = commissionType;
        CommissionAmount = commissionAmount;
        Currency = currency;
        Description = description;

        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }
}
