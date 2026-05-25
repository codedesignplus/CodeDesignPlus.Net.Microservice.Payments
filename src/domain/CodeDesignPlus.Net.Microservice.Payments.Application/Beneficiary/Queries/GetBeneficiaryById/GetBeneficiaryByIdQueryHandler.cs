using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Queries.GetBeneficiaryById;

public class GetBeneficiaryByIdQueryHandler(IBeneficiaryRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetBeneficiaryByIdQuery, BeneficiaryDto>
{
    public async Task<BeneficiaryDto> Handle(GetBeneficiaryByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<BeneficiaryAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.BeneficiaryNotFound);

        return mapper.Map<BeneficiaryDto>(aggregate);
    }
}
