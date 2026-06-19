using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Queries.GetAllPaymentProviderConfigs;

public class GetAllPaymentProviderConfigsQueryHandler(IPaymentProviderConfigRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetAllPaymentProviderConfigsQuery, Pagination<PaymentProviderConfigDto>>
{
    public async Task<Pagination<PaymentProviderConfigDto>> Handle(GetAllPaymentProviderConfigsQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var results = await repository.MatchingAsync<PaymentProviderConfigAggregate>(request.Criteria, user.Tenant, cancellationToken);

        return mapper.Map<Pagination<PaymentProviderConfigDto>>(results);
    }
}
