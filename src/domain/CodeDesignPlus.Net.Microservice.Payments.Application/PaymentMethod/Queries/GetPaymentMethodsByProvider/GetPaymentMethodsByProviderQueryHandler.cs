namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentMethod.Queries.GetPaymentMethodsByProvider;

public class GetPaymentMethodsByProviderQueryHandler(IPaymentMethodRepository repository, IMapper mapper, ICacheManager cacheManager) : IRequestHandler<GetPaymentMethodsByProviderQuery, List<PaymentMethodDto>>
{
    public async Task<List<PaymentMethodDto>> Handle(GetPaymentMethodsByProviderQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var cacheKey = $"PaymentMethodsByProvider-{request.Provider}-{string.Join(",", request.Methods)}";

        var exist = await cacheManager.ExistsAsync(cacheKey);

        if (exist)
            return await cacheManager.GetAsync<List<PaymentMethodDto>>(cacheKey);

        var paymentMethods = await repository.GetByProviderAsync(request.Provider, request.Methods, cancellationToken);

        var data = mapper.Map<List<PaymentMethodDto>>(paymentMethods);

        await cacheManager.SetAsync(cacheKey, data, TimeSpan.FromHours(6));

        return data;
    }
}
