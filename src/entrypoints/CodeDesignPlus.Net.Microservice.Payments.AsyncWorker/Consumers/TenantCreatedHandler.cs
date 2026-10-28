using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.CreatePendingMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.DomainEvents;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Consumers;

/// <summary>
/// Gives every new tenant its collection account, waiting for the keys of its own merchant.
/// </summary>
/// <remarks>
/// The event may arrive more than once: the account's id derives from the tenant, so a repetition finds it already
/// created and does nothing.
/// </remarks>
[QueueName<MerchantAccountAggregate>("TenantCreatedHandler")]
public class TenantCreatedHandler(IMediator mediator, ILogger<TenantCreatedHandler> logger) : IEventHandler<TenantCreatedDomainEvent>
{
    public async Task HandleAsync(TenantCreatedDomainEvent data, CancellationToken token)
    {
        var country = data.Location?.Country?.Alpha2;

        if (string.IsNullOrWhiteSpace(country))
        {
            logger.LogWarning("Tenant {Tenant} was created without a country: it gets no collection account until it connects one.", data.AggregateId);

            return;
        }

        await mediator.Send(new CreatePendingMerchantAccountCommand(data.AggregateId, country, data.CreatedBy), token);
    }
}
