using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Queries.GetBeneficiaryByUserId;

public class GetBeneficiaryByUserIdQueryHandler(IBeneficiaryRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetBeneficiaryByUserIdQuery, BeneficiaryDto>
{
    public async Task<BeneficiaryDto> Handle(GetBeneficiaryByUserIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.GetByUserIdAsync(request.UserId, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.BeneficiaryNotFound);

        return mapper.Map<BeneficiaryDto>(aggregate);
    }
}
