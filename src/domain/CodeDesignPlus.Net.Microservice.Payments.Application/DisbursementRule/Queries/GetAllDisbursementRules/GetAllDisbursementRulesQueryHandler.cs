using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Queries.GetAllDisbursementRules;

public class GetAllDisbursementRulesQueryHandler(IDisbursementRuleRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetAllDisbursementRulesQuery, Pagination<DisbursementRuleDto>>
{
    public async Task<Pagination<DisbursementRuleDto>> Handle(GetAllDisbursementRulesQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var results = await repository.MatchingAsync<DisbursementRuleAggregate>(request.Criteria, user.Tenant, cancellationToken);

        return mapper.Map<Pagination<DisbursementRuleDto>>(results);
    }
}
