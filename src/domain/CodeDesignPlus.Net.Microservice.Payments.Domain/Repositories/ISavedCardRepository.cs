namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;

public interface ISavedCardRepository : IRepositoryBase
{
    Task<List<SavedCardAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<SavedCardAggregate> GetCreditCardsAsync(Guid userId, string cardNumber, string expirationDate, CancellationToken cancellationToken);

    /// <summary>
    /// Guarda el registro nuevo si su id no existe todavía (pendings/589, regla 66).
    /// </summary>
    /// <param name="savedCard">El agregado nuevo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><c>false</c> si otro envío del mismo formulario ya lo guardó: el segundo choca contra la clave primaria.</returns>
    Task<bool> CreateIfAbsentAsync(SavedCardAggregate savedCard, CancellationToken cancellationToken);
}
