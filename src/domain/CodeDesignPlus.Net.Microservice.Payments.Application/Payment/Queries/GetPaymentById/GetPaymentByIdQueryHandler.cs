namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentById;

public class GetPaymentByIdQueryHandler(IPaymentRepository repository, IMapper mapper, IUserContext user, ICacheManager cacheManager) : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exists = await cacheManager.ExistsAsync(request.Id.ToString());

        if (exists)
            return await cacheManager.GetAsync<PaymentDto>(request.Id.ToString());

        var payment = await repository.FindAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(payment, Errors.PaymentNotFound);

        var dto = mapper.Map<PaymentDto>(payment);

        await cacheManager.SetAsync(request.Id.ToString(), payment);

        return dto;
    }
}
