using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ExpireStalePayments;
using CodeDesignPlus.Net.Hangfire.Abstractions;
using CodeDesignPlus.Net.Hangfire.Abstractions.Attributes;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Jobs;

/// <summary>
/// Cierra los cobros que se quedaron en curso y nadie va a terminar.
/// </summary>
/// <remarks>
/// Un comprador abre la pasarela, se va, y el cobro se queda en <c>Initiated</c> para siempre. Nadie vuelve a
/// mirarlo: el proveedor solo avisa de lo que resuelve, no de lo que el comprador abandona.
/// <para>
/// <b>Vive aqui y no en facturacion</b> porque el ciclo de vida del pago es de este servicio. Al cerrarlos, el
/// agregado levanta el mismo evento que un rechazo, que facturacion ya consume para retirar la cotizacion que
/// congelaba el importe a cobrar. <b>Un solo barrido resuelve las dos limpiezas</b>, y no hay que inventar un
/// segundo job que hurgue en el estado de un servicio ajeno.
/// </para>
/// <para>
/// <b>Por que a las 3:00 y no cada pocos minutos.</b> Con un plazo de un dia, adelantar la corrida no cierra
/// nada antes: solo mira mas veces lo mismo. Y se coloca fuera de la ventana de los barridos de facturacion
/// —la mora a las 2:00, los estados de cuenta a las 4:00— para que un cierre no se cruce con el documento que
/// el otro esta leyendo.
/// </para>
/// </remarks>
[RecurringJobOptions("0 3 * * *", jobId: "expire-stale-payments-job")]
public class ExpireStalePaymentsJob(
    IMediator mediator,
    ILogger<ExpireStalePaymentsJob> logger) : IRecurrentJob
{
    /// <summary>
    /// Cuanto se espera antes de dar un cobro por perdido.
    /// </summary>
    /// <remarks>
    /// <b>Generoso a proposito, y el desequilibrio no es simetrico.</b> Una sesion de PSE muere en minutos y
    /// una tarjeta se resuelve en segundos, asi que un dia entero sobra para cualquier cobro legitimo.
    /// <para>
    /// Si se cerrara antes de tiempo y la pasarela contestara despues, el agregado rechazaria la transicion y
    /// el aviso acabaria en la cola de errores: <b>dinero cobrado que no se aplica</b>. Esperar de mas, en
    /// cambio, solo alarga una cotizacion congelada, y eso no bloquea a nadie: facturacion deja de tener en
    /// cuenta una cotizacion pasados 30 minutos, mucho antes de que este barrido llegue a mirarla.
    /// </para>
    /// </remarks>
    private static readonly Duration StaleAfter = Duration.FromDays(1);

    /// <summary>
    /// Cuantos se cierran como mucho en una pasada, para no leer la coleccion entera de golpe.
    /// </summary>
    /// <remarks>
    /// Los que sobren esperan a manana. No hay prisa: el que lleva un dia colgado puede llevar dos, y partir
    /// la lectura evita que un pico raro deje al worker sin memoria.
    /// </remarks>
    private const int BatchSize = 500;

    /// <inheritdoc/>
    [DisableConcurrentExecution(timeoutInSeconds: 5 * 60)]
    public async Task ExecuteAsync(IJobCancellationToken cancellationToken)
    {
        logger.LogInformation("ExpireStalePaymentsJob: cerrando los cobros en curso de mas de {StaleAfter}.", StaleAfter);

        await mediator.Send(new ExpireStalePaymentsCommand(StaleAfter, BatchSize), cancellationToken.ShutdownToken);
    }
}
