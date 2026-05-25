using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Queries.GetAllDisbursements;

public class GetAllDisbursementsQueryHandler(IDisbursementRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetAllDisbursementsQuery, Pagination<DisbursementDto>>
{
    public async Task<Pagination<DisbursementDto>> Handle(GetAllDisbursementsQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var results = await repository.MatchingAsync<DisbursementAggregate>(request.Criteria, user.Tenant, cancellationToken);

        return mapper.Map<Pagination<DisbursementDto>>(results);
    }
}
