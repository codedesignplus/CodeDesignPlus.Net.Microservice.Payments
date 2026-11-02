namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentOutcome;

/// <summary>
/// Reads the outcome of a payment of the session's tenant, or of one the user started.
/// </summary>
/// <remarks>
/// Read from the database and never from a cache, like <c>GetPaymentStatus</c>: the screen asks right after the payment
/// closes. A payment of another tenant answers the same «payment not found» as one that does not exist, so its id does
/// not reveal that it exists; a platform payment (a license) is read by the administrator who started it
/// (<see cref="PaymentAggregate.IsVisibleTo"/>).
/// </remarks>
public class GetPaymentOutcomeQueryHandler(IPaymentRepository repository, IUserContext user) : IRequestHandler<GetPaymentOutcomeQuery, PaymentOutcomeDto>
{
    public async Task<PaymentOutcomeDto> Handle(GetPaymentOutcomeQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var payment = await repository.FindAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsFalse(payment?.IsVisibleTo(user.Tenant, user.IdUser) == true, Errors.PaymentNotFound);

        return PaymentOutcomeDto.From(payment!);
    }
}
