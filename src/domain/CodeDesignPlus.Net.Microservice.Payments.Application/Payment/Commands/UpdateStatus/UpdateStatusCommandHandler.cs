using CodeDesignPlus.Net.Microservice.Payments.Application.Common;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;

public class UpdateStatusCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<UpdateStatusCommand>
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
    }
}