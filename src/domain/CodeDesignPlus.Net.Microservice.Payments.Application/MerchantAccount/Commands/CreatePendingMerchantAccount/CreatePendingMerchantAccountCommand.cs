using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.CreatePendingMerchantAccount;

/// <summary>
/// Gives a new tenant its collection account, waiting for its keys.
/// </summary>
/// <param name="Tenant">The new tenant.</param>
/// <param name="Country">ISO 3166-1 alpha-2 country of the tenant.</param>
/// <param name="CreatedBy">Who created the tenant.</param>
public record CreatePendingMerchantAccountCommand(Guid Tenant, string Country, Guid CreatedBy) : IRequest;

public class CreatePendingMerchantAccountCommandValidator : AbstractValidator<CreatePendingMerchantAccountCommand>
{
    public CreatePendingMerchantAccountCommandValidator()
    {
        RuleFor(x => x.Tenant).NotEmpty();
        RuleFor(x => x.Country).NotEmpty().Length(2);
    }
}

/// <summary>
/// Creates the account once: the id derives from the tenant, so a redelivered event finds the account already there
/// and two concurrent deliveries collide on the primary key instead of creating two.
/// </summary>
public class CreatePendingMerchantAccountCommandHandler(IMerchantAccountRepository repository, IPubSub pubsub, IOptions<PaymentProvidersOptions> options, ILogger<CreatePendingMerchantAccountCommandHandler> logger) : IRequestHandler<CreatePendingMerchantAccountCommand>
{
    public async Task Handle(CreatePendingMerchantAccountCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var provider = options.Value.ProviderFor(request.Country);

        MerchantAccountAggregate created = null!;

        var written = await repository.UpdateWithRetryAsync(async () => await repository.GetByTenantAsync(request.Tenant, cancellationToken) ?? MerchantAccountAggregate.CreatePending(request.Tenant, request.Country, provider, request.CreatedBy), account =>
        {
            // Already stored: this delivery is a repetition.
            if (account.Version > 0)
            {
                return false;
            }

            created = account;

            return true;
        }, cancellationToken);

        if (!written)
        {
            return;
        }

        if (provider is null)
        {
            logger.LogWarning("Tenant {Tenant} got a collection account without provider: no provider is routed for country {Country}.", request.Tenant, request.Country);
        }

        await pubsub.PublishAsync(created.GetAndClearEvents(), cancellationToken);
    }
}
