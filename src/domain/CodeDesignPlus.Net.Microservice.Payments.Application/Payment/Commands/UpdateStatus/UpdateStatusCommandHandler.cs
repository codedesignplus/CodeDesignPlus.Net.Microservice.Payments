using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.gRpc.Clients.Services.Payment;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;

public class UpdateStatusCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, INotificationGrpc notification) : IRequestHandler<UpdateStatusCommand>
{
    public async Task Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        PaymentAggregate payment;

        if (user.Tenant != Guid.Empty)
            payment = await repository.FindAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);
        else
            payment = await repository.FindAsync<PaymentAggregate>(request.Id, cancellationToken);

        payment.SetFinalResponse(request.Status, request.Metadata);

        await repository.UpdateAsync(payment, cancellationToken);

        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

        if (request.Status == CodeDesignPlus.Net.Microservice.Payments.Domain.Enums.PaymentStatus.Failed)
        {
            await notification.SendToUserAsync(new CodeDesignPlus.Net.gRpc.Clients.Services.Notification.NotificationUserRequest
            {
                UserId = payment.Buyer.BuyerId.ToString(),
                EventName = "OrderPaymentFailed",
                Id = request.Id.ToString(),
                SentBy = payment.Buyer.BuyerId.ToString(),
                Tenant = user.Tenant.ToString(),
                JsonPayload = CodeDesignPlus.Net.Serializers.JsonSerializer.Serialize(new
                {
                    message = $"The transaction has failed | {request.Id}"
                })
            }, cancellationToken);
        }
    }
}