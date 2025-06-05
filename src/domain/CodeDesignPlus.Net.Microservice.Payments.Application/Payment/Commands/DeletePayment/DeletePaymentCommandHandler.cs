namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.DeletePayment;

public class DeletePaymentCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<DeletePaymentCommand>
{
    public Task Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}