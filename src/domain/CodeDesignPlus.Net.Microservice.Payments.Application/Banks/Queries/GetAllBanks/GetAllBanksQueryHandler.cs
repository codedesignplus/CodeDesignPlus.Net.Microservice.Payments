namespace CodeDesignPlus.Net.Microservice.Payments.Application.Banks.Queries.GetAllBanks;

public class GetAllBanksQueryHandler(IBankRepository repository, IMapper mapper, ICacheManager cacheManager) : IRequestHandler<GetAllBanksQuery, List<BanksDto>>
{
    private const string CACHE_KEY = "BanksList";

    public async Task<List<BanksDto>> Handle(GetAllBanksQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exist = await cacheManager.ExistsAsync(CACHE_KEY);

        if (exist)        
            return await cacheManager.GetAsync<List<BanksDto>>(CACHE_KEY);
        
        var banks = await repository.GetAllAsync(cancellationToken);

        var data = mapper.Map<List<BanksDto>>(banks);

        await cacheManager.SetAsync(CACHE_KEY, data, TimeSpan.FromHours(6));

        return data;
    }
}
