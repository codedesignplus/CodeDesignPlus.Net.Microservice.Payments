using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Queries.GetAllBeneficiaries;

public class GetAllBeneficiariesQueryHandler(IBeneficiaryRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetAllBeneficiariesQuery, Pagination<BeneficiaryDto>>
{
    public async Task<Pagination<BeneficiaryDto>> Handle(GetAllBeneficiariesQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var results = await repository.MatchingAsync<BeneficiaryAggregate>(request.Criteria, user.Tenant, cancellationToken);

        return mapper.Map<Pagination<BeneficiaryDto>>(results);
    }
}
