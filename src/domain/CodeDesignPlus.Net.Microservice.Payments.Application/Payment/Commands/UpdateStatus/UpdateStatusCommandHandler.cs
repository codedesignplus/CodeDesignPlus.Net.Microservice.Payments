using CodeDesignPlus.Net.Microservice.Payments.Application.Common;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;

public class UpdateStatusCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, IPaymentProviderAdapterFactory adapterFactory) : IRequestHandler<UpdateStatusCommand, PaymentResponseDto>
{
    public async Task<PaymentResponseDto> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        PaymentAggregate payment;

        if (user.Tenant != Guid.Empty)
            payment = await repository.FindAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);
        else
            payment = await repository.FindAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNullOrEmpty(payment.ProviderTransactionId!, Errors.PaymentProviderTransactionIdNotFound);

        var adapter = adapterFactory.GetAdapter(payment.PaymentProvider);

        var result = await adapter.CheckStatusAsync(payment.ProviderTransactionId!, cancellationToken);

        payment.SetResponse(result.Status, result.Message, result.RawResponse, result.FinancialNetwork);

        await repository.UpdateAsync(payment, cancellationToken);

        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

        return result;
    }
}