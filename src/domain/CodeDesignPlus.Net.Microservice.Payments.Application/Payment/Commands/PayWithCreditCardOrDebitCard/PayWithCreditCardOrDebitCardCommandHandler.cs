using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Services;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.PayWithCreditCardOrDebitCard;

public class PayWithCreditCardOrDebitCardCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, IPayment payment) : IRequestHandler<PayWithCreditCardOrDebitCardCommand>
{
    public async Task Handle(PayWithCreditCardOrDebitCardCommand request, CancellationToken cancellationToken)
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