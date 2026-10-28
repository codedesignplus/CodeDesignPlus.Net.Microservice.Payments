namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentStatus;

/// <summary>
/// How a payment stands, read from the database and never from a cache: a status that is read to decide what to do
/// next cannot be the one it had when it was first cached.
/// </summary>
/// <param name="Id">Id of the payment.</param>
public record GetPaymentStatusQuery(Guid Id) : IRequest<PaymentStatusDto>;

public class GetPaymentStatusQueryHandler(IPaymentRepository repository, IUserContext user) : IRequestHandler<GetPaymentStatusQuery, PaymentStatusDto>
{
    public async Task<PaymentStatusDto> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var payment = user.Tenant != Guid.Empty
            ? await repository.FindAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken)
            : await repository.FindAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(payment, Errors.PaymentNotFound);

        return PaymentStatusDto.From(payment);
    }
}
