using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.DisconnectMerchantAccount;

/// <summary>
/// Disconnects the collection account of the tenant in session and deletes its keys.
/// </summary>
public record DisconnectMerchantAccountCommand : IRequest;

/// <summary>
/// Deletes the keys from the Vault before the account says it has none, so no account ever claims to be disconnected
/// while its keys are still readable.
/// </summary>
public class DisconnectMerchantAccountCommandHandler(IMerchantAccountRepository repository, IMerchantSecretStore secretStore, IUserContext user, IPubSub pubsub) : IRequestHandler<DisconnectMerchantAccountCommand>
{
    public async Task Handle(DisconnectMerchantAccountCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(user.Tenant, Errors.TenantIsRequired);

        var tenant = user.Tenant;

        var current = await repository.GetByTenantAsync(tenant, cancellationToken);

        ApplicationGuard.IsTrue(current is null || current.Status is not (MerchantAccountStatus.Active or MerchantAccountStatus.Invalid), Domain.Errors.MerchantAccountIsNotConnected);

        await secretStore.DeleteAsync(current!.Id, cancellationToken);

        MerchantAccountAggregate saved = null!;

        var written = await repository.UpdateWithRetryAsync(async () => (await repository.GetByTenantAsync(tenant, cancellationToken))!, account =>
        {
            if (account.Status == MerchantAccountStatus.Disabled)
            {
                return false;
            }

            account.Disconnect(user.IdUser);

            saved = account;

            return true;
        }, cancellationToken);

        if (written)
        {
            await pubsub.PublishAsync(saved.GetAndClearEvents(), cancellationToken);
        }
    }
}
