
namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;

/// <summary>
/// Pone el token de la pasarela cuando se paga con una tarjeta guardada (pendings/180).
/// </summary>
/// <remarks>
/// El navegador nunca recibe el token de una tarjeta guardada: para pagar con ella manda su id en el lugar del token.
/// Aquí se busca la tarjeta, se comprueba que sea de quien paga y se cambia el id por el token. Si el valor no es el id
/// de una tarjeta guardada, es el token de una tarjeta recién tokenizada y se deja como llegó.
/// </remarks>
public static class SavedCardToken
{
    /// <summary>
    /// Devuelve el medio de pago con el token de la tarjeta guardada, o el mismo medio si no se paga con una.
    /// </summary>
    /// <param name="paymentMethod">El medio de pago de la petición.</param>
    /// <param name="repository">El repositorio de tarjetas guardadas.</param>
    /// <param name="user">El usuario que paga.</param>
    /// <param name="cancellationToken">El token de cancelación.</param>
    public static async Task<ValueObjects.Payment.PaymentMethod> ResolveAsync(ValueObjects.Payment.PaymentMethod paymentMethod, ISavedCardRepository repository, IUserContext user, CancellationToken cancellationToken)
    {
        var creditCard = paymentMethod.CreditCard;

        if (creditCard is null || !Guid.TryParse(creditCard.Token, out var savedCardId))
            return paymentMethod;

        var savedCard = await repository.FindAsync<SavedCardAggregate>(savedCardId, cancellationToken);

        if (savedCard is null)
            return paymentMethod;

        ApplicationGuard.IsTrue(savedCard.UserId != user.IdUser, Errors.SavedCardNotFound);

        var resolved = ValueObjects.Payment.CreditCard.Create(savedCard.Token, savedCard.Last4Digits, savedCard.ExpirationDate, savedCard.CardHolderName, creditCard.SecurityCode, creditCard.InstallmentsNumber);

        return ValueObjects.Payment.PaymentMethod.Create(paymentMethod.Type, paymentMethod.Pse, resolved);
    }
}
