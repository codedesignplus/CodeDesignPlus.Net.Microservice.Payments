using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Queries.GetDisbursementRuleById;

public class GetDisbursementRuleByIdQueryHandler(IDisbursementRuleRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetDisbursementRuleByIdQuery, DisbursementRuleDto>
{
    public async Task<DisbursementRuleDto> Handle(GetDisbursementRuleByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<DisbursementRuleAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.DisbursementRuleNotFound);

        return mapper.Map<DisbursementRuleDto>(aggregate);
    }
}
