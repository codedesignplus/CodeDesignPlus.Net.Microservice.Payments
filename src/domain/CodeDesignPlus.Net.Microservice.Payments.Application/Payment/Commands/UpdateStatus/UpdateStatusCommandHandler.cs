using CodeDesignPlus.Net.gRpc.Clients.Abstractions;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;

public class UpdateStatusCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, ILiveChannelGrpc liveChannel) : IRequestHandler<UpdateStatusCommand>
{
    public async Task Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        PaymentAggregate payment;

        if (user.Tenant != Guid.Empty)
            payment = await repository.FindAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);
        else
            payment = await repository.FindAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(payment, Errors.PaymentNotFound);

        payment.SetFinalResponse(request.Status, request.Metadata, request.ProviderTransactionId, request.CapturedAmount, request.PaymentMethodType);

        await repository.UpdateAsync(payment, cancellationToken);

        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

        if (request.Status == CodeDesignPlus.Net.Microservice.Payments.Domain.Enums.PaymentStatus.Failed)
        {
            // Efimero: el desenlace del pago ya es consultable. La pantalla de compra reconcilia
            // contra GET Order/{id}, asi que quien no estuviera mirando se entera igual al volver,
            // y guardar esto solo llenaria la bandeja de ruido que nadie abre.
            //
            // El nombre del evento y la forma del payload no se tocan: /purchase/processing escucha
            // "OrderPaymentFailed" y lee { message }. Renombrarlo aqui no rompe nada visible — el
            // push sale, el acuse dice success, y la pantalla se queda esperando para siempre.
            await liveChannel.PushToUserAsync(
                payment.Buyer.BuyerId,
                "OrderPaymentFailed",
                new
                {
                    message = $"The transaction has failed | {request.Id}"
                },
                user.Tenant,
                cancellationToken: cancellationToken);
        }
    }
}
