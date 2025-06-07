using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Services;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;

public class Payment(IServiceProvider serviceProvider) : IPayment
{
    public Task<Domain.Models.TransactionResponse> ProcessPayment(Guid id, Transaction transaction, Provider provider, CancellationToken cancellationToken)
    {
        if (provider == Provider.Payu)
        {
            var payu = serviceProvider.GetRequiredService<IPayu>();

            return payu.ProcessPayment(id, transaction, provider, cancellationToken);
        }

        throw new NotSupportedException($"Provider {provider} is not supported.");
    }
}
