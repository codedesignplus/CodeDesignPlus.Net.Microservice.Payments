using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, ILogger<InitiatePaymentCommandHandler> logger, IPaymentProviderAdapterFactory adapterFactory) : IRequestHandler<InitiatePaymentCommand, PaymentResponseDto>
{
    public async Task<PaymentResponseDto> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        bool exist;

        if (user.Tenant != Guid.Empty)
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);
        else
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.PaymentAlredyExists);

        var adapter = adapterFactory.GetAdapter(request.PaymentProvider);

        var payment = PaymentAggregate.Create(
            request.Id,
            request.Module,
            request.SubTotal,
            request.Tax,
            request.Total,
            request.Payer,
            request.PaymentMethod,
            request.Description,
            request.PaymentProvider,
            user.Tenant,
            user.IdUser
        );

        await repository.CreateAsync(payment, cancellationToken);

        var providerResult = await adapter.InitiatePaymentAsync(request, cancellationToken);

        payment.SetTransactionId(providerResult.TransactionId);

        await repository.UpdateAsync(payment, cancellationToken);

        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

        return providerResult;
    }
}