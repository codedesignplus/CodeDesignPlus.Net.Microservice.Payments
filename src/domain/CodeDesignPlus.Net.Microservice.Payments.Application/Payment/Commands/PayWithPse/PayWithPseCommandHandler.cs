namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.PayWithPse;

public class PayWithPseCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<PayWithPseCommand>
{
    public Task Handle(PayWithPseCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}