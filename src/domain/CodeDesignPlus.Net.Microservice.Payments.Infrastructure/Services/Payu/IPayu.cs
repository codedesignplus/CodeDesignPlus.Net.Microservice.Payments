using CodeDesignPlus.Net.Microservice.Payments.Domain.Services;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

public interface IPayu : IPayment
{
    Task<BankResponse?> GetBanksListAsync(CancellationToken cancellationToken);
}
