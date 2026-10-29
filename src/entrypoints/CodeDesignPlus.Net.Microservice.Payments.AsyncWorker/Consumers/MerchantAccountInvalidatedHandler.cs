using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Notifications;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Consumers;

/// <summary>
/// Tells the administration that its collection account stopped working: online payments are off until someone
/// replaces the keys (rule 55).
/// </summary>
/// <remarks>
/// The notice goes to the configured roles of the tenant, and the pending task is visible where it is solved: the
/// account shows as invalid on its screen. Its id derives from the event, so a redelivery does not repeat it.
/// </remarks>
[QueueName<MerchantAccountAggregate>("MerchantAccountInvalidatedHandler")]
public class MerchantAccountInvalidatedHandler(IInboxGrpc inbox, IOptions<MerchantAccountNoticesOptions> options) : IEventHandler<MerchantAccountInvalidatedDomainEvent>
{
    public Task HandleAsync(MerchantAccountInvalidatedDomainEvent data, CancellationToken token)
    {
        var notices = options.Value;

        return inbox.NotifyRolesAsync(
            notices.Roles.Select(x => x.ToString()),
            notices.Kind,
            "La cuenta de recaudo dejó de funcionar",
            $"{new Error(data.ReasonCode).GetMessage(notices.Language)} El pago en línea quedó suspendido: revisa las llaves en el panel de {data.Provider}, reemplázalas y valida la conexión.",
            module: notices.Module,
            aggregateId: data.AggregateId.ToString(),
            payload: new { accountId = data.AggregateId, provider = data.Provider, reasonCode = data.ReasonCode },
            tenant: data.Tenant,
            sentBy: notices.SentBy,
            notificationId: NotificationIds.For(data.EventId, notices.Kind),
            cancellationToken: token);
    }
}
