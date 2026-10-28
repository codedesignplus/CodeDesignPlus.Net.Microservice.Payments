using CodeDesignPlus.Net.Hangfire.Abstractions;
using CodeDesignPlus.Net.Hangfire.Abstractions.Attributes;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ReconcilePayments;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Jobs;

/// <summary>
/// Recovers the payments whose provider event never reached the webhook.
/// </summary>
/// <remarks>
/// The provider retries an event a few times and gives up. Without this run, a payment collected while the webhook was
/// down would stay in progress until the expiration sweep closed it as unpaid: money charged and never applied. Every
/// ten minutes keeps that window short; the payments younger than the configured minimum age are skipped so the payer
/// still on the hosted page is not polled.
/// </remarks>
[RecurringJobOptions("*/10 * * * *", jobId: "reconcile-payments-job")]
public class ReconcilePaymentsJob(IMediator mediator, ILogger<ReconcilePaymentsJob> logger) : IRecurrentJob
{
    /// <inheritdoc/>
    [DisableConcurrentExecution(timeoutInSeconds: 5 * 60)]
    public async Task ExecuteAsync(IJobCancellationToken cancellationToken)
    {
        logger.LogInformation("ReconcilePaymentsJob: asking the providers about the payments in progress.");

        await mediator.Send(new ReconcilePaymentsCommand(), cancellationToken.ShutdownToken);
    }
}
