using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Services;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;

public class PayCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, IPayment payment, ILogger<PayCommandHandler> logger) : IRequestHandler<PayCommand>
{
    public async Task Handle(PayCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        logger.LogWarning("Processing payment for request: {@Request}", request);

        bool exist;
        
        if (user.Tenant != Guid.Empty)
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);
        else
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.PaymentAlredyExists);

        var response = await payment.ProcessPayment(request.Id, request.Transaction, Provider.Payu, cancellationToken);

        var requestJson = CodeDesignPlus.Net.Serializers.JsonSerializer.Serialize(response.Request);

        var aggregate = PaymentAggregate.Create(request.Id, Provider.Payu, request.Transaction, request.Module, requestJson, response.Response, user.Tenant, user.IdUser);

        await repository.CreateAsync(aggregate, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}