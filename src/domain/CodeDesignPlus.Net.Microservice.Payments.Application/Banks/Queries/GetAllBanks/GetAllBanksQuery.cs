namespace CodeDesignPlus.Net.Microservice.Payments.Application.Banks.Queries.GetAllBanks;

public record GetAllBanksQuery() : IRequest<List<BanksDto>>;

