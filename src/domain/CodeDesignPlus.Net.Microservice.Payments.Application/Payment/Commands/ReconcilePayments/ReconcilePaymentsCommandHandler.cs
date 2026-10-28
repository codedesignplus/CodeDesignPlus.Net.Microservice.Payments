using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.RefreshPaymentStatus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ReconcilePayments;

/// <summary>
/// Recovers the outcome of payments whose event never reached the webhook.
/// </summary>
/// <remarks>
/// The provider retries an event a few times and then gives up; a webhook that was down for that window would leave
/// a paid payment in progress until the expiration sweep closed it as unpaid. This run asks the provider about every
/// payment in progress older than <see cref="PaymentProvidersOptions.ReconciliationMinimumAge"/>, so the payer still
/// on the hosted page is not polled.
/// <para>
/// A payment that fails does not stop the rest, but the run throws at the end if any did: carrying on and reporting
/// success are different things, and the job panel must show the failure.
/// </para>
/// </remarks>
public class ReconcilePaymentsCommandHandler(IPaymentRepository repository, IMediator mediator, IOptions<PaymentProvidersOptions> options, ILogger<ReconcilePaymentsCommandHandler> logger) : IRequestHandler<ReconcilePaymentsCommand>
{
    public async Task Handle(ReconcilePaymentsCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var cutoff = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromTimeSpan(options.Value.ReconciliationMinimumAge));

        var inProgress = await repository.GetInProgressOlderThanAsync(cutoff, options.Value.ReconciliationBatchSize, cancellationToken);

        var closed = 0;
        var failed = 0;

        foreach (var payment in inProgress)
        {
            try
            {
                var status = await mediator.Send(new RefreshPaymentStatusCommand(payment.Id), cancellationToken);

                if (status.Status != payment.Status)
                {
                    closed++;
                }
            }
            catch (Exception exception)
            {
                failed++;

                logger.LogError(exception, "Payment {PaymentId} of tenant {Tenant} could not be reconciled with {Provider}.", payment.Id, payment.Tenant, payment.Provider);
            }
        }

        logger.LogInformation("Reconciliation of payments in progress before {Cutoff}: {Checked} checked, {Closed} closed, {Failed} failed.", cutoff, inProgress.Count, closed, failed);

        if (failed > 0)
        {
            throw new InvalidOperationException($"{failed} payment(s) in progress could not be reconciled.");
        }
    }
}
