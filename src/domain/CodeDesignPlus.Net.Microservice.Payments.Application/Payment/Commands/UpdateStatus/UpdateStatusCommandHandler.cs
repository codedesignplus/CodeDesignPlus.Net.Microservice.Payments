using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Fees;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;

/// <summary>
/// Closes a payment and publishes its outcome, with the commission the provider keeps when the payment succeeded
/// (pendings/660, stage 6): the tenant's books need it the day the payment is approved.
/// </summary>
/// <remarks>
/// A failed attempt the provider still lets the payer retry does not close the payment (pendings/622): it waits as
/// <see cref="PaymentStatus.AwaitingRetry"/> and the modules hear about the rejection without applying its consequences. While
/// that window is open another failure changes nothing; once it ends, the reconciliation brings the failure back and the
/// payment closes.
/// </remarks>
public class UpdateStatusCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, ILiveChannelGrpc liveChannel, IProviderFeeCalculator fees) : IRequestHandler<UpdateStatusCommand>
{
    public async Task Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        PaymentAggregate payment;

        if (user.Tenant != Guid.Empty)
            payment = await repository.FindAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);
        else
            payment = await repository.FindAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(payment, Errors.PaymentNotFound);

        var fee = request.Status == PaymentStatus.Succeeded
            ? await fees.ForAsync(payment, request.CapturedAmount, request.PaymentMethodType, request.ReportedFee, cancellationToken)
            : null;

        var now = SystemClock.Instance.GetCurrentInstant();
        var wasAwaitingRetry = payment.Status == PaymentStatus.AwaitingRetry;

        if (request.Status == PaymentStatus.Failed && payment.IsAwaitingRetryAt(now))
        {
            return;
        }

        if (request.Status == PaymentStatus.Failed && payment.Status is (PaymentStatus.Initiated or PaymentStatus.Pending) && request.RetryableUntil is { } retryableUntil && now < retryableUntil)
        {
            payment.DeclineAttempt(request.Metadata, request.ProviderTransactionId, request.PaymentMethodType, request.FailureReason, request.ProviderMessage, retryableUntil);
        }
        else
        {
            payment.SetFinalResponse(request.Status, request.Metadata, request.ProviderTransactionId, request.CapturedAmount, request.PaymentMethodType, fee, request.FailureReason, request.ProviderMessage);
        }

        await repository.UpdateAsync(payment, cancellationToken);

        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

        // The payer hears about the rejection once: when the attempt is declined, not again when its retry window closes.
        if (request.Status == PaymentStatus.Failed && !wasAwaitingRetry)
        {
            // Efimero: el desenlace del pago ya es consultable. La pantalla de compra reconcilia
            // contra GET Order/{id}, asi que quien no estuviera mirando se entera igual al volver,
            // y guardar esto solo llenaria la bandeja de ruido que nadie abre.
            //
            // El nombre del evento y la forma del payload no se tocan: /purchase/processing escucha
            // "OrderPaymentFailed" y lee { message }. Renombrarlo aqui no rompe nada visible — el
            // push sale, el acuse dice success, y la pantalla se queda esperando para siempre.
            await liveChannel.PushToUserAsync(
                payment.Buyer.BuyerId,
                "OrderPaymentFailed",
                new
                {
                    message = $"The transaction has failed | {request.Id}"
                },
                user.Tenant,
                cancellationToken: cancellationToken);
        }
    }
}
