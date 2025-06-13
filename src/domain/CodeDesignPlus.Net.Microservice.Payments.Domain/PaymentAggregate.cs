using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class PaymentAggregate(Guid id) : AggregateRootBase(id)
{
    public Provider Provider { get; private set; } = Provider.None;
    public Transaction Transaction { get; private set; } = null!;
    public string Request { get; private set; } = null!;
    public string Response { get; private set; } = null!;
    public Guid? Tenant { get; private set; }

    private PaymentAggregate(Guid id, Provider provider, Transaction transaction, string request, string response, Guid? tenant, Guid createdBy) : this(id)
    {
        DomainGuard.IsNull(transaction, Errors.TransactionCannotBeNull);
        DomainGuard.IsNull(request, Errors.RequestCannotBeNull);
        DomainGuard.IsNull(response, Errors.ResponseCannotBeNull);

        Provider = provider;
        Transaction = transaction;
        Request = request;
        Response = response;

        CreatedBy = createdBy;
        CreatedAt = SystemClock.Instance.GetCurrentInstant();
        IsActive = true;

        if (tenant.HasValue)
            Tenant = tenant.Value;

        base.AddEvent(PaymentCompletedDomainEvent.Create(Id, Provider, Transaction, Request, Response, Tenant));
    }

    public static PaymentAggregate Create(Guid id, Provider provider, Transaction transaction, string request, string response, Guid tenant, Guid createdBy)
    {
        return new PaymentAggregate(id, provider, transaction, request, response, tenant, createdBy);
    }
}
