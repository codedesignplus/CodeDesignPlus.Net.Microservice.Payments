namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.DisconnectMerchantAccount;

/// <summary>
/// Disconnects the collection account of the tenant in session and deletes its keys.
/// </summary>
public record DisconnectMerchantAccountCommand : IRequest;

/// <summary>
/// Disconnecting forgets the ciphertext of the keys: without it the Vault has nothing to decrypt.
/// </summary>
public class DisconnectMerchantAccountCommandHandler(IMerchantAccountRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<DisconnectMerchantAccountCommand>
{
    public async Task Handle(DisconnectMerchantAccountCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(user.Tenant, Errors.TenantIsRequired);

        var tenant = user.Tenant;

        var current = await repository.GetByTenantAsync(tenant, cancellationToken);

        ApplicationGuard.IsTrue(current is null || current.Status is not (MerchantAccountStatus.Active or MerchantAccountStatus.Invalid), Domain.Errors.MerchantAccountIsNotConnected);

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
