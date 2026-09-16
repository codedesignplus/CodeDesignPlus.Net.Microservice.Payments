using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ExpireStalePayments;

/// <summary>
/// Manejador de <see cref="ExpireStalePaymentsCommand"/>.
/// </summary>
/// <remarks>
/// Un cobro que nadie termina se queda en curso para siempre, y el modulo que lo pidio sigue esperandolo. En
/// facturacion eso deja viva la cotizacion que congela el importe, y con ella el saldo del propietario
/// retenido: la mora de ese documento no vuelve a moverse y un segundo intento de pago no puede reemplazarla.
/// <para>
/// <b>Cerrarlos aqui resuelve las dos limpiezas de una vez.</b> Al expirarlos, el agregado levanta el mismo
/// evento que un rechazo, que facturacion <b>ya</b> consume para retirar la cotizacion de ese pago. No hace
/// falta un segundo barrido en facturacion que hurgue en el estado de un servicio que no es suyo.
/// </para>
/// <para>
/// <b>Un cobro que falle no puede detener al resto</b>, asi que cada uno va en su propio intento; pero al
/// terminar se lanza si alguno cayo. Continuar y decir que todo fue bien son cosas distintas (regla 23): sin
/// esto, la corrida sale en verde en el panel con todos los cobros sin cerrar.
/// </para>
/// </remarks>
public class ExpireStalePaymentsCommandHandler(
    IPaymentRepository repository,
    IPubSub pubsub,
    ILogger<ExpireStalePaymentsCommandHandler> logger) : IRequestHandler<ExpireStalePaymentsCommand>
{
    public async Task Handle(ExpireStalePaymentsCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var cutoff = SystemClock.Instance.GetCurrentInstant().Minus(request.OlderThan);

        var stale = await repository.GetInProgressOlderThanAsync(cutoff, request.BatchSize, cancellationToken);

        if (stale.Count == 0)
        {
            logger.LogInformation("No hay cobros en curso anteriores a {Cutoff}.", cutoff);

            return;
        }

        var expired = 0;
        var failed = 0;

        foreach (var payment in stale)
        {
            try
            {
                payment.Expire($"Sin respuesta de la pasarela en {request.OlderThan}.");

                // El agregado antes que el evento: si la publicacion falla, el cobro ya esta cerrado y el
                // barrido de manana no vuelve a encontrarlo, pero facturacion no se ha enterado y su
                // cotizacion sigue viva. Al reves seria peor —anunciar un cierre que no llego a escribirse—
                // asi que el orden es este, y el hueco no atrapa a nadie: facturacion deja de tener en cuenta
                // una cotizacion pasados 30 minutos, sin necesidad de este evento.
                await repository.UpdateAsync(payment, cancellationToken);

                await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

                expired++;
            }
            catch (Exception exception)
            {
                failed++;

                logger.LogError(
                    exception,
                    "No se pudo cerrar el cobro {PaymentId} de la copropiedad {Tenant}, iniciado el {CreatedAt}.",
                    payment.Id, payment.Tenant, payment.CreatedAt);
            }
        }

        logger.LogInformation(
            "Cobros en curso anteriores a {Cutoff}: {Expired} cerrado(s), {Failed} fallido(s).",
            cutoff, expired, failed);

        if (failed > 0)
            throw new InvalidOperationException($"No se pudieron cerrar {failed} cobro(s) en curso.");
    }
}
