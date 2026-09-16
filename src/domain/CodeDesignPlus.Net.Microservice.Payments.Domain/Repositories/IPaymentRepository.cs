namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

public interface IPaymentRepository : IRepositoryBase
{
    /// <summary>
    /// Los cobros que siguen en curso y ya son mas viejos que el corte.
    /// </summary>
    /// <remarks>
    /// Mira <c>CreatedAt</c> y no <c>UpdatedAt</c>: lo que importa es cuanto lleva el cobro sin resolverse
    /// desde que nacio, y las actualizaciones intermedias —anotar la respuesta de la pasarela al
    /// iniciarlo— no son senales de vida del comprador. Con <c>UpdatedAt</c>, un cobro cuya redireccion
    /// se anoto tarde se libraria del barrido sin motivo.
    /// </remarks>
    /// <param name="cutoff">La frontera: se devuelven los creados antes de este instante.</param>
    /// <param name="limit">Cuantos como mucho, para que el barrido no se lea la coleccion entera de golpe.</param>
    /// <param name="cancellationToken">Token de cancelacion.</param>
    Task<List<PaymentAggregate>> GetInProgressOlderThanAsync(Instant cutoff, int limit, CancellationToken cancellationToken);
}