using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Queries.GetDisbursementsByPayment;

public class GetDisbursementsByPaymentQueryHandler(IDisbursementRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetDisbursementsByPaymentQuery, List<DisbursementDto>>
{
    public async Task<List<DisbursementDto>> Handle(GetDisbursementsByPaymentQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var results = await repository.GetByPaymentIdAsync(request.PaymentId, user.Tenant, cancellationToken);

        return mapper.Map<List<DisbursementDto>>(results);
    }
}
