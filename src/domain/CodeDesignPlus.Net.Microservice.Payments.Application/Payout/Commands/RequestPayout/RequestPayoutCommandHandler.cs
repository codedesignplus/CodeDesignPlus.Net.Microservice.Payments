using CodeDesignPlus.Net.Microservice.Payments.Application.Payout.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payout.Commands.RequestPayout;

/// <summary>
/// Registers a payout request. Sending it to the provider and closing it by its event come in a later stage; until
/// then the payout stays <see cref="PayoutStatus.Requested"/>.
/// </summary>
/// <remarks>
/// A repeated request with the same id answers the payout already registered instead of creating another: the caller
/// may retry after a timeout without paying twice.
/// </remarks>
public class RequestPayoutCommandHandler(IPayoutRepository repository, IMerchantAccountRepository accountRepository, IBeneficiaryRepository beneficiaryRepository, IPubSub pubsub, ILogger<RequestPayoutCommandHandler> logger) : IRequestHandler<RequestPayoutCommand, PayoutDto>
{
    public async Task<PayoutDto> Handle(RequestPayoutCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var existing = await repository.FindAsync<PayoutAggregate>(request.Id, cancellationToken);

        if (existing is not null)
        {
            ApplicationGuard.IsTrue(existing.Tenant != request.Tenant, Errors.PayoutNotFound);

            return PayoutDto.From(existing);
        }

        var account = await accountRepository.GetByTenantAsync(request.Tenant, cancellationToken);

        ApplicationGuard.IsTrue(account is null || account.Status != MerchantAccountStatus.Active, Errors.MerchantAccountIsNotActive);
        ApplicationGuard.IsFalse(account!.PayoutsEnabled, Errors.PayoutsAreNotEnabled);

        var beneficiary = await beneficiaryRepository.FindAsync<BeneficiaryAggregate>(request.BeneficiaryId, request.Tenant, cancellationToken);

        ApplicationGuard.IsTrue(beneficiary is null || beneficiary.Tenant != request.Tenant || !beneficiary.IsActive, Errors.BeneficiaryNotFound);

        var payout = PayoutAggregate.Request(request.Id, request.Tenant, request.Module, request.ReferenceId, request.Purpose, request.BeneficiaryId, request.Amount, request.Description, request.RequestedBy);

        var created = await repository.CreateIfAbsentAsync(payout, cancellationToken);

        if (!created)
        {
            logger.LogInformation("Payout {PayoutId} was already requested by a concurrent call.", request.Id);

            var stored = await repository.FindAsync<PayoutAggregate>(request.Id, cancellationToken);

            return PayoutDto.From(stored);
        }

        await pubsub.PublishAsync(payout.GetAndClearEvents(), cancellationToken);

        return PayoutDto.From(payout);
    }
}
