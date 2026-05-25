using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Queries.GetDisbursementById;

public class GetDisbursementByIdQueryHandler(IDisbursementRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetDisbursementByIdQuery, DisbursementDto>
{
    public async Task<DisbursementDto> Handle(GetDisbursementByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<DisbursementAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.DisbursementNotFound);

        return mapper.Map<DisbursementDto>(aggregate);
    }
}
