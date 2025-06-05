namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdatePayment;

public class UpdatePaymentCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<UpdatePaymentCommand>
{
    public Task Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}