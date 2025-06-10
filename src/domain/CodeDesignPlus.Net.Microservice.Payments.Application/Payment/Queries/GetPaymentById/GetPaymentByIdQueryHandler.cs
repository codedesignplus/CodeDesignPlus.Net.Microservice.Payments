namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentById;

public class GetPaymentByIdQueryHandler(IPaymentRepository repository, IMapper mapper, IUserContext user, ICacheManager cacheManager) : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exists = await cacheManager.ExistsAsync(request.Id.ToString());

        if (exists)
            return await cacheManager.GetAsync<PaymentDto>(request.Id.ToString());

        var license = await repository.FindAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(license, Errors.PaymentNotFound);

        var dto = mapper.Map<PaymentDto>(license);

        await cacheManager.SetAsync(request.Id.ToString(), license);

        return dto;
    }
}
