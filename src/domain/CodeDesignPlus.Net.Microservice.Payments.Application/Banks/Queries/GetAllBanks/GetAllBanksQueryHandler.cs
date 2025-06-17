using Microsoft.Extensions.Hosting;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Banks.Queries.GetAllBanks;

public class GetAllBanksQueryHandler(IBankRepository repository, IMapper mapper, ICacheManager cacheManager, IHostEnvironment hostEnvironment) : IRequestHandler<GetAllBanksQuery, List<BanksDto>>
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

        if(!hostEnvironment.IsProduction())
            data.Add(new BanksDto
            {
                Id = Guid.Parse("442cdb72-3acd-4cf0-9264-a505cd139d41"),
                Name = "FINANCIAL_INSTITUTION_CODE",
                Description = "This is a test bank for development purposes.",
                Code = "1022",
                IsActive = true
            });

        await cacheManager.SetAsync(CACHE_KEY, data, TimeSpan.FromHours(6));

        return data;
    }
}
