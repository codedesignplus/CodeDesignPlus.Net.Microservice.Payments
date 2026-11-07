namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPlatformPaymentById;

/// <summary>
/// Handles <see cref="GetPlatformPaymentByIdQuery"/>.
/// </summary>
/// <remarks>
/// A tenant's payment answers like one that does not exist: this route only reads the platform's.
/// </remarks>
public class GetPlatformPaymentByIdQueryHandler(IPaymentRepository repository, IMapper mapper) : IRequestHandler<GetPlatformPaymentByIdQuery, PlatformPaymentDto>
{
    /// <summary>
    /// Reads the payment.
    /// </summary>
    /// <param name="request">The id of the payment.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The payment as the Super Admin reads it.</returns>
    public async Task<PlatformPaymentDto> Handle(GetPlatformPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var payment = await repository.FindAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsFalse(payment?.PlatformAccount == true, Errors.PaymentNotFound);

        return mapper.Map<PlatformPaymentDto>(payment!);
    }
}
