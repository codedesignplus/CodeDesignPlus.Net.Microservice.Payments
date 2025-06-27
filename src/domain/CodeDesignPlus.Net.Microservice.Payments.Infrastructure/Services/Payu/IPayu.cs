using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Models;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

public interface IPayu : IPaymentProviderAdapter
{
    Task<BankResponse?> GetBanksListAsync(CancellationToken cancellationToken);
}
