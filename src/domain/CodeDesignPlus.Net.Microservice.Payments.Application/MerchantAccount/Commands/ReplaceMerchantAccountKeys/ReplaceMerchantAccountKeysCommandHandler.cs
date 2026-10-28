using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ReplaceMerchantAccountKeys;

/// <summary>
/// Replaces the keys of the connected collection account of the tenant in session.
/// </summary>
/// <remarks>
/// The new keys are validated before they replace the old ones in the Vault, so wrong keys leave the working ones in
/// place.
/// </remarks>
public class ReplaceMerchantAccountKeysCommandHandler(IMerchantAccountRepository repository, IMerchantAccountConnector connector, IUserContext user, IPubSub pubsub, IMerchantSecretsCache cache, IOptions<PaymentProvidersOptions> options) : IRequestHandler<ReplaceMerchantAccountKeysCommand, MerchantAccountDto>
{
    public async Task<MerchantAccountDto> Handle(ReplaceMerchantAccountKeysCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(user.Tenant, Errors.TenantIsRequired);

        var tenant = user.Tenant;

        var current = await repository.GetByTenantAsync(tenant, cancellationToken);

        ApplicationGuard.IsTrue(current is null || current.Status is not (MerchantAccountStatus.Active or MerchantAccountStatus.Invalid), Domain.Errors.MerchantAccountIsNotConnected);

        var connection = await connector.ValidateAndStoreAsync(current!, request.ToKeys(), cancellationToken);

        MerchantAccountAggregate saved = null!;

        await repository.UpdateWithRetryAsync(async () => (await repository.GetByTenantAsync(tenant, cancellationToken))!, account =>
        {
            account.ReplaceKeys(connection, user.IdUser);

            saved = account;

            return true;
        }, cancellationToken);

        cache.Forget(saved.Id);

        await pubsub.PublishAsync(saved.GetAndClearEvents(), cancellationToken);

        return MerchantAccountDto.From(saved, options.Value.EventsUrl);
    }
}
