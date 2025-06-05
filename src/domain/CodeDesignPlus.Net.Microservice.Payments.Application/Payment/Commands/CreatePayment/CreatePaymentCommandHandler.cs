namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.CreatePayment;

public class CreatePaymentCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<CreatePaymentCommand>
{
    public Task Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}