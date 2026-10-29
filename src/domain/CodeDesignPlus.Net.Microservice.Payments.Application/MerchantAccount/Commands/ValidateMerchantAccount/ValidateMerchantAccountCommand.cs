using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ValidateMerchantAccount;

/// <summary>
/// Checks the stored keys of the collection account of the tenant in session against the provider again.
/// </summary>
public record ValidateMerchantAccountCommand : IRequest<MerchantAccountDto>;

/// <summary>
/// Leaves the account <see cref="MerchantAccountStatus.Active"/> when the stored keys work and
/// <see cref="MerchantAccountStatus.Invalid"/> when the provider rejects them.
/// </summary>
/// <remarks>
/// The secrets are decrypted from the stored ciphertext, never from the cache: this is the check that must see what is
/// really stored. A provider or a Vault that does not answer leaves the account as it was and the error asks to retry:
/// not answering is not rejecting.
/// </remarks>
public class ValidateMerchantAccountCommandHandler(IMerchantAccountRepository repository, IMerchantSecretCipher cipher, IMerchantSecretsCache cache, IPaymentProviderFactory providers, IUserContext user, IPubSub pubsub, IOptions<PaymentProvidersOptions> options) : IRequestHandler<ValidateMerchantAccountCommand, MerchantAccountDto>
{
    public async Task<MerchantAccountDto> Handle(ValidateMerchantAccountCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(user.Tenant, Errors.TenantIsRequired);

        var tenant = user.Tenant;

        var current = await repository.GetByTenantAsync(tenant, cancellationToken);

        ApplicationGuard.IsTrue(current is null || current.Status is not (MerchantAccountStatus.Active or MerchantAccountStatus.Invalid) || current.Secrets is null, Domain.Errors.MerchantAccountIsNotConnected);

        var secrets = await cipher.OpenAsync(tenant, current!.Secrets!, cancellationToken);

        var validation = await providers.GetCredentials(current.Provider!).ValidateAsync(current.PublicKey!, secrets, cancellationToken);

        var works = validation.IsValid && validation.Environment == options.Value.Environment;
        var reason = validation.IsValid ? Errors.MerchantKeysEnvironmentMismatch : validation.Reason ?? Errors.MerchantKeysWereRejected;

        MerchantAccountAggregate saved = null!;

        await repository.UpdateWithRetryAsync(async () => (await repository.GetByTenantAsync(tenant, cancellationToken))!, account =>
        {
            if (works)
            {
                account.ConfirmKeys(user.IdUser);
            }
            else
            {
                account.Invalidate(reason.Code, user.IdUser);
            }

            saved = account;

            return true;
        }, cancellationToken);

        cache.Forget(saved.Id);

        await pubsub.PublishAsync(saved.GetAndClearEvents(), cancellationToken);

        return MerchantAccountDto.From(saved, options.Value.EventsUrl);
    }
}
