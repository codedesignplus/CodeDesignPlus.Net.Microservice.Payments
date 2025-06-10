using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPayment;

public class GetAllPaymentQueryHandler(IPaymentRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetAllPaymentQuery, Pagination<PaymentDto>>
{
    public async Task<Pagination<PaymentDto>> Handle(GetAllPaymentQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var licenses = await repository.MatchingAsync<PaymentAggregate>(request.Criteria, user.Tenant, cancellationToken);

        return mapper.Map<Pagination<PaymentDto>>(licenses);
    }
}
