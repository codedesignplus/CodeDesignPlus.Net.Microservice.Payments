using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Services;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;

public class PayCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, IPayment payment) : IRequestHandler<PayCommand>
{
    public async Task Handle(PayCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.PaymentAlredyExists);

        var response = await payment.ProcessPayment(request.Id, request.Transaction, Provider.Payu, cancellationToken);

        var aggregate = PaymentAggregate.Create(request.Id, Provider.Payu, request.Transaction, response.Request, response.Response, user.Tenant, user.IdUser);

        await repository.CreateAsync(aggregate, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}