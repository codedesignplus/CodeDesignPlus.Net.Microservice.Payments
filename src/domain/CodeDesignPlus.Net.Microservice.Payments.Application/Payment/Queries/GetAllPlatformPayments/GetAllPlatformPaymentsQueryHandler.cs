using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPlatformPayments;

/// <summary>
/// Handles <see cref="GetAllPlatformPaymentsQuery"/>.
/// </summary>
/// <remarks>
/// Only the platform's payments: the condition goes in the filter (<see cref="PlatformScopedCriteria"/>), so a tenant's
/// payment never reaches this screen whatever the client filters. Who may call it is decided by the RBAC, not here.
/// </remarks>
public class GetAllPlatformPaymentsQueryHandler(IPaymentRepository repository, IMapper mapper) : IRequestHandler<GetAllPlatformPaymentsQuery, Pagination<PlatformPaymentDto>>
{
    /// <summary>
    /// Reads the page of platform payments.
    /// </summary>
    /// <param name="request">The query with the screen's criteria.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The page of platform payments.</returns>
    public async Task<Pagination<PlatformPaymentDto>> Handle(GetAllPlatformPaymentsQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var scoped = PlatformScopedCriteria.Scope(request.Criteria);

        var payments = await repository.MatchingAsync<PaymentAggregate>(scoped, cancellationToken);

        return mapper.Map<Pagination<PlatformPaymentDto>>(payments);
    }
}
