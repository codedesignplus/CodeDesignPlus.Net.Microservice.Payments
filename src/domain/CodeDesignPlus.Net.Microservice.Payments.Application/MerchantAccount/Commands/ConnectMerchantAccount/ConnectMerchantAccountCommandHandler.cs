using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ConnectMerchantAccount;

/// <summary>
/// Connects the collection account of the tenant in session.
/// </summary>
/// <remarks>
/// A tenant created before collection accounts existed has none: it is created here, routed by the country of the
/// tenant in session, exactly as the consumer of the tenant's creation would have done.
/// </remarks>
public class ConnectMerchantAccountCommandHandler(IMerchantAccountRepository repository, IMerchantAccountConnector connector, IUserContext user, Security.Abstractions.ITenant tenantContext, IPubSub pubsub, IMerchantSecretsCache cache, IOptions<PaymentProvidersOptions> options) : IRequestHandler<ConnectMerchantAccountCommand, MerchantAccountDto>
{
    public async Task<MerchantAccountDto> Handle(ConnectMerchantAccountCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(user.Tenant, Errors.TenantIsRequired);

        var tenant = user.Tenant;

        var current = await LoadAsync(tenant, cancellationToken);

        // Checked before storing anything: connecting over a connected account would overwrite the keys that work.
        ApplicationGuard.IsTrue(current.Status is not (MerchantAccountStatus.PendingConnection or MerchantAccountStatus.Disabled), Domain.Errors.MerchantAccountIsAlreadyConnected);

        var connection = await connector.ValidateAndStoreAsync(current, request.ToKeys(), cancellationToken);

        MerchantAccountAggregate saved = null!;

        await repository.UpdateWithRetryAsync(() => LoadAsync(tenant, cancellationToken), account =>
        {
            account.Connect(connection, user.IdUser);

            saved = account;

            return true;
        }, cancellationToken);

        cache.Forget(saved.Id);

        await pubsub.PublishAsync(saved.GetAndClearEvents(), cancellationToken);

        return MerchantAccountDto.From(saved, options.Value.EventsUrl);
    }

    private async Task<MerchantAccountAggregate> LoadAsync(Guid tenant, CancellationToken cancellationToken)
    {
        var account = await repository.GetByTenantAsync(tenant, cancellationToken)
            ?? MerchantAccountAggregate.CreatePending(tenant, tenantContext.Country.Alpha2, options.Value.ProviderFor(tenantContext.Country.Alpha2), user.IdUser);

        // The country may have been routed after the account was created.
        if (account.Provider is null && options.Value.ProviderFor(account.Country) is { } provider)
        {
            account.AssignProvider(provider);
        }

        return account;
    }
}
