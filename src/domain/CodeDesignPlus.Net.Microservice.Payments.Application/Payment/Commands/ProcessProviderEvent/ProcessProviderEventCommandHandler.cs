using CodeDesignPlus.Net.Exceptions;
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
/// When the signature does not match, the provider is asked about the payment with the private key. If it reports the
/// same transaction and status, the event was genuine and it is the stored events secret that is wrong: the outcome is
/// applied from the provider's answer and the account counts a rejection. A forged event does not match the provider's
/// answer, so it can neither apply anything nor invalidate an account.
/// </para>
/// <para>
/// An event that cannot be trusted is discarded and logged, never applied: an unknown reference (another
/// environment, or a forged one), an account without keys, or a wrong signature. The webhook still answers 200 so the
/// provider does not keep retrying something that will never be accepted.
/// </para>
/// </remarks>
public class ProcessProviderEventCommandHandler(IPaymentRepository repository, IPaymentProviderFactory providers, IMerchantCredentialsResolver credentialsResolver, IMerchantAccountHealth health, IMediator mediator, IPubSub pubsub, ILogger<ProcessProviderEventCommandHandler> logger) : IRequestHandler<ProcessProviderEventCommand>
{
    public async Task Handle(ProcessProviderEventCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var collect = providers.GetCollect(request.Provider);

        var notification = await collect.ReadNotificationAsync(request.Body, cancellationToken);

        if (notification is null)
        {
            await ProcessRefundAsync(collect, request.Body, cancellationToken);

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

        var outcome = notification.Outcome;

        if (!collect.IsAuthentic(request.Body, credentials))
        {
            var confirmed = await ConfirmWithProviderAsync(collect, payment, credentials, outcome, cancellationToken);

            if (confirmed is null)
            {
                logger.LogWarning("Event from {Provider} for payment {PaymentId} of tenant {Tenant} discarded: its signature does not match the account's secret.", collect.Code, paymentId, payment.Tenant);

                return;
            }

            await health.ReportRejectionAsync(payment, Errors.MerchantEventsSecretDoesNotVerify, cancellationToken);

            outcome = confirmed;
        }
        else
        {
            await health.ReportAcceptanceAsync(payment, credentials, cancellationToken);
        }

        if (!outcome.IsFinal)
        {
            logger.LogInformation("Event from {Provider} for payment {PaymentId}: still {Status}.", collect.Code, paymentId, outcome.Status);

            return;
        }

        await mediator.Send(new UpdateStatusCommand(payment.Id, outcome.Status, outcome.Raw, outcome.TransactionId, outcome.Amount, outcome.PaymentMethodType, outcome.ReportedFee), cancellationToken);
    }

    /// <summary>
    /// Applies a refund event: it is trusted the same way as a payment event, by the signature of the account of the
    /// payment it refunds, and then recorded on the payment.
    /// </summary>
    /// <remarks>
    /// The refund's own reference is not necessarily the payment's, so the payment is looked up by it when it is one and by
    /// the refunded transaction otherwise. A forged or unknown refund is discarded and logged, like a payment event.
    /// </remarks>
    private async Task ProcessRefundAsync(ICollectProvider collect, string body, CancellationToken cancellationToken)
    {
        var refunds = providers.FindRefunds(collect.Code);

        var refund = refunds is null ? null : await refunds.ReadRefundNotificationAsync(body, cancellationToken);

        if (refund is null)
        {
            logger.LogInformation("Event from {Provider} discarded: it is neither a payment nor a refund event.", collect.Code);

            return;
        }

        PaymentAggregate? payment = null;

        if (Guid.TryParse(refund.Reference, out var paymentId))
        {
            payment = await repository.FindAsync<PaymentAggregate>(paymentId, cancellationToken);
        }

        if (payment is null && !string.IsNullOrWhiteSpace(refund.TransactionId))
        {
            payment = await repository.FindByProviderTransactionAsync(collect.Code, refund.TransactionId, cancellationToken);
        }

        if (payment is null || !string.Equals(payment.Provider, collect.Code, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Refund {RefundId} from {Provider} discarded: no payment of that provider for transaction {TransactionId}.", refund.RefundId, collect.Code, refund.TransactionId);

            return;
        }

        var credentials = await credentialsResolver.ForPaymentAsync(payment, cancellationToken);

        if (credentials is null || !collect.IsAuthentic(body, credentials))
        {
            logger.LogWarning("Refund {RefundId} from {Provider} for payment {PaymentId} of tenant {Tenant} discarded: it cannot be verified with the account's secret.", refund.RefundId, collect.Code, payment.Id, payment.Tenant);

            return;
        }

        if (payment.Status != PaymentStatus.Succeeded)
        {
            logger.LogWarning("Refund {RefundId} from {Provider} discarded: payment {PaymentId} is {Status}, not succeeded.", refund.RefundId, collect.Code, payment.Id, payment.Status);

            return;
        }

        if (!payment.RegisterRefund(refund.RefundId, refund.Status, refund.Amount, refund.Code, refund.Message))
        {
            logger.LogInformation("Refund {RefundId} of payment {PaymentId}: already known as {Status}.", refund.RefundId, payment.Id, refund.Status);

            return;
        }

        await repository.UpdateAsync(payment, cancellationToken);

        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

        logger.LogInformation("Refund {RefundId} of payment {PaymentId} of tenant {Tenant}: {Status}.", refund.RefundId, payment.Id, payment.Tenant, refund.Status);
    }

    /// <summary>
    /// What the provider says about the payment, when it matches the unverified event; null otherwise.
    /// </summary>
    private async Task<CollectOutcome?> ConfirmWithProviderAsync(ICollectProvider collect, PaymentAggregate payment, MerchantCredentials credentials, CollectOutcome claimed, CancellationToken cancellationToken)
    {
        try
        {
            var known = await collect.GetOutcomeAsync(payment, credentials, cancellationToken);

            return known is not null && known.TransactionId == claimed.TransactionId && known.Status == claimed.Status ? known : null;
        }
        catch (CodeDesignPlusException exception) when (health.IsRejection(exception))
        {
            await health.ReportRejectionAsync(payment, Errors.MerchantPrivateKeyWasRejected, cancellationToken);

            return null;
        }
    }
}
