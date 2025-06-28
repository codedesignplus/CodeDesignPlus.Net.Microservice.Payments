using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;

[DtoGenerator]
public record InitiatePaymentCommand(Guid Id, string Module, Amount SubTotal, Amount Tax, Amount Total, string Description, Payer Payer, Domain.ValueObjects.PaymentMethod PaymentMethod, PaymentProvider PaymentProvider ) : IRequest<PaymentResponseDto>;

public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Module).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PaymentProvider).IsInEnum().NotEqual(PaymentProvider.None);

        RuleFor(x => x.SubTotal)
            .NotNull().WithMessage("SubTotal cannot be null.")
            .SetValidator(new AmountDtoValidator());

        RuleFor(x => x.Tax)
            .NotNull().WithMessage("Tax cannot be null.")
            .SetValidator(new AmountDtoValidator());

        RuleFor(x => x.Total)
            .NotNull().WithMessage("Total cannot be null.")
            .SetValidator(new AmountDtoValidator());

        RuleFor(x => x.Payer).NotNull().SetValidator(new PayerInfoDtoValidator());
    }
}

public class PaymentMethodInfoDtoValidator : AbstractValidator<Domain.ValueObjects.PaymentMethod>
{
    public PaymentMethodInfoDtoValidator()
    {
        RuleFor(x => x.CreditCard).SetValidator(new CreditCardInfoDtoValidator()).When(x => x.CreditCard != null);
        RuleFor(x => x.Pse).SetValidator(new PseInfoDtoValidator()).When(x => x.Pse != null);

        RuleFor(x => x)
            .Must(x => (x.CreditCard != null) ^ (x.Pse != null))
            .WithMessage("Either CreditCard or Pse information must be provided, but not both.");
    }
}

public class AmountDtoValidator : AbstractValidator<Amount>
{
    public AmountDtoValidator()
    {
        RuleFor(x => x.Value).GreaterThan(0).WithMessage("Amount value must be greater than zero.");
        RuleFor(x => x.Currency)
        .NotEmpty()
        .Length(3)
        .Matches(@"^[A-Z]{3}$")
        .WithMessage("Currency must be a three-letter uppercase ISO 4217 code.")
        .When(x => x.Currency is not null);
    }
}

public class AddressDtoValidator : AbstractValidator<Address>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().Length(2).Matches(@"^[A-Z]{2}$").WithMessage("Country must be a two-letter uppercase ISO 3166-1 alpha-2 code.");
        RuleFor(x => x.State).NotEmpty().MaximumLength(40);
        RuleFor(x => x.City).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PostalCode).NotEmpty().Matches(@"^\d{1,8}$").MaximumLength(8);
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^\+?\d{1,11}$").MaximumLength(11);
    }
}

public class PayerInfoDtoValidator : AbstractValidator<Payer>
{
    public PayerInfoDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.EmailAddress).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DniNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DniType).NotEmpty().MaximumLength(3);
        RuleFor(x => x.BillingAddress)
            .NotNull()
            .SetValidator(new AddressDtoValidator())
            .When(x => x.BillingAddress != null);
    }
}

public class CreditCardInfoDtoValidator : AbstractValidator<CreditCard?>
{
    public CreditCardInfoDtoValidator()
    {
        RuleFor(x => x!.Number).NotEmpty().Length(13, 20).CreditCard();
        RuleFor(x => x!.SecurityCode).NotEmpty().Length(3, 4);
        RuleFor(x => x!.ExpirationDate).NotEmpty().Length(7).Matches(@"^\d{4}/\d{2}$").WithMessage("Expiration date must be in YYYY/MM format.");
        RuleFor(x => x!.Name).NotEmpty().MaximumLength(255);
    }
}

public class PseInfoDtoValidator : AbstractValidator<Pse?>
{
    public PseInfoDtoValidator()
    {
        RuleFor(x => x!.PseCode).NotEmpty().MaximumLength(34);
        RuleFor(x => x!.TypePerson).NotEmpty().MaximumLength(2); // "N" o "J"
        RuleFor(x => x!.PseResponseUrl).NotEmpty().MaximumLength(255).Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("Must be a valid URL.");
    }
}

