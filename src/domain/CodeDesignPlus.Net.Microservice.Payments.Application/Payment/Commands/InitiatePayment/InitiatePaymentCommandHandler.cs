using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, IPaymentProviderAdapterFactory adapterFactory) 
    : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResponseDto>
{
    public async Task<InitiatePaymentResponseDto> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        bool exist;

        if (user.Tenant != Guid.Empty)
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);
        else
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.PaymentAlredyExists);

        var payment = PaymentAggregate.Create(
            request.Id,
            request.Module,
            request.ReferenceId,
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

        var adapter = adapterFactory.GetAdapter(request.PaymentProvider);

        var providerResponse = await adapter.InitiatePaymentAsync(payment, cancellationToken);

        var responseDictionary = new Dictionary<string, string?> { { "redirectUrl", providerResponse.RedirectUrl } };
        payment.SetInitiateResponse(responseDictionary);

        await repository.UpdateAsync(payment, cancellationToken);

        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

        return new InitiatePaymentResponseDto
        {
            PaymentId = payment.Id,
            Success = providerResponse.Success ,
            NextAction = providerResponse.NextAction,
            WidgetParameters = providerResponse.WidgetParameters,
            ProviderResponse = providerResponse.ProviderResponse,
            RedirectUrl = providerResponse.RedirectUrl,
            ProviderTransactionId = providerResponse.ProviderTransactionId
        };
    }
}