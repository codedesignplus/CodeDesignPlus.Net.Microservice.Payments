using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class PaymentMethodAggregate(Guid id) : AggregateRootBase(id)
{
    public Provider Provider { get; private set; }
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public TypePaymentMethod Type { get; private set; } = TypePaymentMethod.None;
    public string? Comments { get; private set; } = null!;

    private PaymentMethodAggregate(Guid id, Provider provider, string name, string code, TypePaymentMethod type, string? comments)
        : this(id)
    {
        DomainGuard.GuidIsEmpty(id, Errors.IdPaymentMethodCannotBeEmpty);
        DomainGuard.IsNullOrEmpty(name, Errors.NameOfPaymentMethodCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(name.Length, 64, Errors.NameOfPaymentMethodCannotBeGreaterThan64Characters);
        DomainGuard.IsNullOrEmpty(code, Errors.CodeOfPaymentMethodCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(code.Length, 32, Errors.CodeOfPaymentMethodCannotBeGreaterThan32Characters);
        DomainGuard.IsGreaterThan(comments?.Length ?? 0, 124, Errors.CommentsOfPaymentMethodCannotBeGreaterThan124Characters);

        Provider = provider;
        Name = name;
        Code = code;
        Type = type;
        Comments = comments;
        IsActive = true;

        CreatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    public static PaymentMethodAggregate Create(Guid id, Provider provider, string name, string code, TypePaymentMethod type, string? comments)
    {
        return new PaymentMethodAggregate(id, provider, name, code, type, comments);
    }

    public void Update(Provider provider, string name, string code, TypePaymentMethod type, string? comments, bool isActive)
    {
        DomainGuard.IsNullOrEmpty(name, Errors.NameOfPaymentMethodCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(code, Errors.CodeOfPaymentMethodCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(code.Length, 32, Errors.CodeOfPaymentMethodCannotBeGreaterThan32Characters);

        Provider = provider;
        Name = name;
        Code = code;
        Type = type;
        Comments = comments;
        IsActive = isActive;

        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }
}
