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
/// down would stay in progress until the expiration sweep closed it as unpaid: money charged and never applied. The
/// payments younger than the configured minimum age are skipped so the payer still on the hosted page is not polled.
/// <para>
/// También es lo que cierra el cobro cuya página de pago venció sin transacción en el proveedor (pendings/622), y por eso
/// corre cada minuto: los módulos que esperan la respuesta (una reserva de parqueadero, un pase de visita) se cansan a los
/// 20 minutos, y el cobro tiene que cerrarse antes, cerca de un minuto después de su página de 15. Cada diez minutos habría
/// cerrado algunos a los 25. Las consultas de más tienen tope: solo se pregunta por los cobros en curso que pasan la edad
/// mínima, y los abandonados salen del grupo en cuanto vencen.
/// </para>
/// </remarks>
[RecurringJobOptions("* * * * *", jobId: "reconcile-payments-job")]
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
