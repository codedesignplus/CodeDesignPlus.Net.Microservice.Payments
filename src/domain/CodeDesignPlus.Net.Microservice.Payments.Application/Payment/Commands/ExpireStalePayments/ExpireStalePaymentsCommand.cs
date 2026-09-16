namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ExpireStalePayments;

/// <summary>
/// Cierra como expirados los cobros que llevan demasiado tiempo en curso.
/// </summary>
/// <remarks>
/// No lleva tenant: el barrido es transversal a todas las copropiedades a proposito. Un cobro colgado no
/// pertenece a la que lo pidio mas que a ninguna otra, y recorrer tenant por tenant obligaria a mantener aqui
/// una lista de tenants que ya vive en otro sitio.
/// </remarks>
/// <param name="OlderThan">Cuanto tiene que llevar un cobro sin resolverse para darlo por perdido.</param>
/// <param name="BatchSize">Cuantos se cierran como mucho en una pasada.</param>
public record ExpireStalePaymentsCommand(Duration OlderThan, int BatchSize) : IRequest;

public class Validator : AbstractValidator<ExpireStalePaymentsCommand>
{
    public Validator()
    {
        // Un plazo de cero cerraria el cobro que acaba de iniciarse, con el comprador todavia en el banco.
        RuleFor(x => x.OlderThan).Must(x => x > Duration.Zero);
        RuleFor(x => x.BatchSize).GreaterThan(0);
    }
}
