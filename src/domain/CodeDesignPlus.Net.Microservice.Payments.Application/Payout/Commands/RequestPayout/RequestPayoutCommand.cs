using CodeDesignPlus.Net.Microservice.Payments.Application.Payout.DataTransferObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payout.Commands.RequestPayout;

/// <summary>
/// Asks to pay a third party out of the tenant's collection account.
/// </summary>
/// <remarks>
/// The tenant travels in the request and not only in the header: the call may come from a worker without a user
/// token.
/// </remarks>
/// <param name="Id">Id generated once by the calling module; repeating it does not pay twice.</param>
/// <param name="Tenant">Tenant whose collection account pays.</param>
/// <param name="Module">Module that asks for the payout.</param>
/// <param name="ReferenceId">Id of what is being paid in the calling module.</param>
/// <param name="Purpose">Why the money leaves the account.</param>
/// <param name="BeneficiaryId">Beneficiary registered in this service.</param>
/// <param name="Amount">Amount to pay.</param>
/// <param name="Description">What is being paid.</param>
/// <param name="RequestedBy">User who asked for the payout.</param>
public record RequestPayoutCommand(Guid Id, Guid Tenant, string Module, Guid ReferenceId, PayoutPurpose Purpose, Guid BeneficiaryId, Money Amount, string Description, Guid RequestedBy) : IRequest<PayoutDto>;

public class RequestPayoutCommandValidator : AbstractValidator<RequestPayoutCommand>
{
    public RequestPayoutCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Tenant).NotEmpty();
        RuleFor(x => x.Module).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReferenceId).NotEmpty();
        RuleFor(x => x.Purpose).IsInEnum();
        RuleFor(x => x.BeneficiaryId).NotEmpty();
        RuleFor(x => x.Amount).NotNull();
        RuleFor(x => x.Amount.Amount).GreaterThan(0).When(x => x.Amount is not null);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(FieldLength.Description);
        RuleFor(x => x.RequestedBy).NotEmpty();
    }
}
