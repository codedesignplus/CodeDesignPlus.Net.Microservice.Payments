using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class DisbursementRuleAggregate(Guid id) : AggregateRootBase(id)
{
    public CommissionType CommissionType { get; private set; }

    /// <summary>Comisión fija en unidades menores. Solo aplica cuando <see cref="CommissionType"/> es Fixed.</summary>
    public long? FixedCommission { get; private set; }

    /// <summary>Moneda ISO 4217 de <see cref="FixedCommission"/>. Solo aplica cuando la comisión es fija.</summary>
    public string? Currency { get; private set; }

    /// <summary>Comisión en puntos base (200 = 2%). Solo aplica cuando <see cref="CommissionType"/> es Percentage.</summary>
    public int? CommissionBasisPoints { get; private set; }

    public string? Description { get; private set; }
    public Guid Tenant { get; private set; }

    public static DisbursementRuleAggregate Create(
        Guid id,
        CommissionType commissionType,
        long? fixedCommission,
        string? currency,
        int? commissionBasisPoints,
        string? description,
        Guid tenant)
    {
        DomainGuard.GuidIsEmpty(id, Errors.PaymentIdCannotBeEmpty);

        Validate(commissionType, fixedCommission, currency, commissionBasisPoints);

        var aggregate = new DisbursementRuleAggregate(id)
        {
            CommissionType = commissionType,
            FixedCommission = fixedCommission,
            Currency = currency,
            CommissionBasisPoints = commissionBasisPoints,
            Description = description,
            Tenant = tenant,
            IsActive = true,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        return aggregate;
    }

    public void Update(
        CommissionType commissionType,
        long? fixedCommission,
        string? currency,
        int? commissionBasisPoints,
        string? description)
    {
        Validate(commissionType, fixedCommission, currency, commissionBasisPoints);

        CommissionType = commissionType;
        FixedCommission = fixedCommission;
        Currency = currency;
        CommissionBasisPoints = commissionBasisPoints;
        Description = description;

        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    /// <summary>Calcula la comisión de la plataforma sobre un importe en unidades menores.</summary>
    public long CalculateCommission(long amount)
    {
        if (CommissionType == CommissionType.Fixed)
            return FixedCommission!.Value;

        return (long)Math.Round(amount * CommissionBasisPoints!.Value / 10000m, MidpointRounding.AwayFromZero);
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    private static void Validate(CommissionType commissionType, long? fixedCommission, string? currency, int? commissionBasisPoints)
    {
        if (commissionType == CommissionType.Fixed)
        {
            DomainGuard.IsTrue(fixedCommission is null or <= 0, Errors.CommissionAmountMustBePositive);
            DomainGuard.IsNullOrEmpty(currency, Errors.DisbursementRuleCurrencyIsRequired);
        }
        else
        {
            DomainGuard.IsTrue(commissionBasisPoints is null or <= 0, Errors.CommissionBasisPointsMustBePositive);
        }
    }
}
