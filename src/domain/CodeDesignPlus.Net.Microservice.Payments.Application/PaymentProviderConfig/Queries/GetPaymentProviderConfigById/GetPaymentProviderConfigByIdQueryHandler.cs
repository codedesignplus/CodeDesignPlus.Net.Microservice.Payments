using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Queries.GetPaymentProviderConfigById;

public class GetPaymentProviderConfigByIdQueryHandler(IPaymentProviderConfigRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetPaymentProviderConfigByIdQuery, PaymentProviderConfigDto>
{
    public async Task<PaymentProviderConfigDto> Handle(GetPaymentProviderConfigByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<PaymentProviderConfigAggregate>(request.Id, user.Tenant, cancellationToken);
        ApplicationGuard.IsNull(aggregate, Errors.PaymentProviderConfigNotFound);

        return mapper.Map<PaymentProviderConfigDto>(aggregate);
    }
}
