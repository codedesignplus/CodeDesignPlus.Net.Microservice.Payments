using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Models;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class PaymentAggregate(Guid id) : AggregateRootBase(id)
{
    public Provider Provider { get; private set; } = Provider.None;
    public Transaction Transaction { get; private set; } = null!;
    public string Module { get; private set; } = null!;
    public string Request { get; private set; } = null!;
    public TransactionResponseData Response { get; private set; } = null!;
    public Guid? Tenant { get; private set; }

    private PaymentAggregate(Guid id, Provider provider, Transaction transaction, string module, string request, TransactionResponseData response, Guid? tenant, Guid createdBy) : this(id)
    {
        DomainGuard.IsNullOrEmpty(module, Errors.ModuleCannotBeNullOrEmpty);
        DomainGuard.IsNull(transaction, Errors.TransactionCannotBeNull);
        DomainGuard.IsNull(request, Errors.RequestCannotBeNull);
        DomainGuard.IsNull(response, Errors.ResponseCannotBeNull);

        Provider = provider;
        Transaction = transaction;
        Request = request;
        Response = response;
        Module = module;

        CreatedBy = createdBy;
        CreatedAt = SystemClock.Instance.GetCurrentInstant();
        IsActive = true;

        if (tenant.HasValue)
            Tenant = tenant.Value;

        base.AddEvent(PaymentCompletedDomainEvent.Create(Id, Provider, Transaction, Request, Response, Tenant));
    }

    public static PaymentAggregate Create(Guid id, Provider provider, Transaction transaction, string module, string request, TransactionResponseData response, Guid tenant, Guid createdBy)
    {
        return new PaymentAggregate(id, provider, transaction, module, request, response, tenant, createdBy);
    }
}
