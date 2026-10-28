using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ProcessProviderEvent;

/// <summary>
/// Applies an event posted to the single webhook that every merchant account shares.
/// </summary>
/// <remarks>
/// Each merchant signs its events with its own secret, so the event cannot be verified before knowing whose it is.
/// Only its reference is read first: it is the id of the payment, which leads to the account and to that account's
/// secret. Only once the signature checks out is anything else in the event believed.
/// <para>
/// An event that cannot be trusted is discarded and logged, never applied: an unknown reference (another
/// environment, or a forged one), an account without keys, or a wrong signature. The webhook still answers 200 so the
/// provider does not keep retrying something that will never be accepted.
/// </para>
/// </remarks>
public class ProcessProviderEventCommandHandler(IPaymentRepository repository, IPaymentProviderFactory providers, IMerchantCredentialsResolver credentialsResolver, IMediator mediator, ILogger<ProcessProviderEventCommandHandler> logger) : IRequestHandler<ProcessProviderEventCommand>
{
    public async Task Handle(ProcessProviderEventCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var collect = providers.GetCollect(request.Provider);

        var notification = await collect.ReadNotificationAsync(request.Body, cancellationToken);

        if (notification is null)
        {
            logger.LogInformation("Event from {Provider} discarded: it is not a payment event.", collect.Code);

            return;
        }

        if (!Guid.TryParse(notification.Reference, out var paymentId))
        {
            logger.LogWarning("Event from {Provider} discarded: its reference {Reference} is not a payment of this service.", collect.Code, notification.Reference);

            return;
        }

        var payment = await repository.FindAsync<PaymentAggregate>(paymentId, cancellationToken);

        if (payment is null || !string.Equals(payment.Provider, collect.Code, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Event from {Provider} discarded: no payment {PaymentId} of that provider.", collect.Code, paymentId);

            return;
        }

        var credentials = await credentialsResolver.ForPaymentAsync(payment, cancellationToken);

        if (credentials is null)
        {
            logger.LogWarning("Event from {Provider} for payment {PaymentId} of tenant {Tenant} discarded: the collection account has no keys to verify it.", collect.Code, paymentId, payment.Tenant);

            return;
        }

        if (!collect.IsAuthentic(request.Body, credentials))
        {
            logger.LogWarning("Event from {Provider} for payment {PaymentId} of tenant {Tenant} discarded: its signature does not match the account's secret.", collect.Code, paymentId, payment.Tenant);

            return;
        }

        var outcome = notification.Outcome;

        if (!outcome.IsFinal)
        {
            logger.LogInformation("Event from {Provider} for payment {PaymentId}: still {Status}.", collect.Code, paymentId, outcome.Status);

            return;
        }

        await mediator.Send(new UpdateStatusCommand(payment.Id, outcome.Status, outcome.Raw, outcome.TransactionId, outcome.Amount, outcome.PaymentMethodType), cancellationToken);
    }
}
