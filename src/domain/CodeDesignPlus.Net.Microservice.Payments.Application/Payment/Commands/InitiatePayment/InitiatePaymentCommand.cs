using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;

/// <summary>
/// Starts a payment: the payer continues on the provider's hosted page, where they choose how to pay.
/// </summary>
/// <param name="Id">Id generated once by the calling module.</param>
/// <param name="Module">Module that charges.</param>
/// <param name="ReferenceId">Id of what is being charged in the calling module.</param>
/// <param name="SubTotal">Amount before taxes.</param>
/// <param name="Tax">Taxes.</param>
/// <param name="Total">Amount to charge.</param>
/// <param name="Description">What is being charged.</param>
/// <param name="Buyer">Who pays; prefills the hosted page.</param>
/// <param name="Provider">Provider code; empty lets the tenant's collection account decide.</param>
/// <param name="ReturnUrl">Page the provider returns to; empty falls back to the configured one.</param>
/// <param name="PlatformAccount">Whether the payment goes to the platform's own account.</param>
[DtoGenerator]
public record InitiatePaymentCommand(
    Guid Id,
    string Module,
    Guid ReferenceId,
    Amount SubTotal,
    Amount Tax,
    Amount Total,
    string Description,
    ValueObjects.User.Buyer Buyer,
    string? Provider,
    string? ReturnUrl,
    bool PlatformAccount
) : IRequest<InitiatePaymentResponseDto>;

public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Module).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReferenceId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Provider).MaximumLength(50);

        RuleFor(x => x.ReturnUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
            .WithErrorCode(Errors.UrlIsInvalid.Code)
            .When(x => !string.IsNullOrWhiteSpace(x.ReturnUrl));

        RuleFor(x => x.SubTotal)
            .NotNull()
            .SetValidator(new AmountDtoValidator());

        RuleFor(x => x.Tax)
            .NotNull()
            .SetValidator(new TaxAmountDtoValidator());

        RuleFor(x => x.Total)
            .NotNull()
            .SetValidator(new AmountDtoValidator());

        RuleFor(x => x.Buyer).NotNull().SetValidator(new BuyerInfoDtoValidator());
    }
}

public class AmountDtoValidator : AbstractValidator<Amount>
{
    public AmountDtoValidator()
    {
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.Currency)
        .NotEmpty()
        .Length(3)
        .Matches(@"^[A-Z]{3}$")
        .WithErrorCode(Errors.CurrencyFormatIsInvalid.Code)
        .When(x => x.Currency is not null);
    }
}

public class TaxAmountDtoValidator : AbstractValidator<Amount>
{
    public TaxAmountDtoValidator()
    {
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency)
        .NotEmpty()
        .Length(3)
        .Matches(@"^[A-Z]{3}$")
        .WithErrorCode(Errors.CurrencyFormatIsInvalid.Code)
        .When(x => x.Currency is not null);
    }
}

/// <summary>
/// The payer as the module knows them. Email and phone are optional: a visitor paying at the front desk is known only by
/// name and document (pendings/665), and the provider's hosted page asks for what is missing. When they come, they are
/// validated as before.
/// </summary>
public class BuyerInfoDtoValidator : AbstractValidator<ValueObjects.User.Buyer>
{
    public BuyerInfoDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.Document).MaximumLength(20);
    }
}
