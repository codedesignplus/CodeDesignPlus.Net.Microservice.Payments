using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Payment;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;

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
    ValueObjects.User.Payer? Payer,
    ValueObjects.Payment.PaymentMethod PaymentMethod,
    PaymentProvider PaymentProvider
) : IRequest<InitiatePaymentResponseDto>;

public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Module).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReferenceId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PaymentProvider).IsInEnum().NotEqual(Domain.Enums.PaymentProvider.None);

        RuleFor(x => x.SubTotal)
            .NotNull().WithMessage("SubTotal cannot be null.")
            .SetValidator(new AmountDtoValidator());

        RuleFor(x => x.Tax)
            .NotNull().WithMessage("Tax cannot be null.")
            .SetValidator(new AmountDtoValidator());

        RuleFor(x => x.Total)
            .NotNull().WithMessage("Total cannot be null.")
            .SetValidator(new AmountDtoValidator());

        RuleFor(x => x.Buyer).NotNull().SetValidator(new BuyerInfoDtoValidator());
        RuleFor(x => x.Payer).SetValidator(new PayerInfoDtoValidator()).When(x => x.Payer != null);
    }
}

public class PaymentMethodInfoDtoValidator : AbstractValidator<ValueObjects.Payment.PaymentMethod>
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

public class AddressDtoValidator : AbstractValidator<ValueObjects.User.Address?>
{
    public AddressDtoValidator()
    {
        When(x => x != null, () =>
        {
            RuleFor(x => x!.Street).NotEmpty().MaximumLength(100);
            RuleFor(x => x!.Country).NotEmpty().Length(2).Matches(@"^[A-Z]{2}$").WithMessage("Country must be a two-letter uppercase ISO 3166-1 alpha-2 code.");
            RuleFor(x => x!.State).NotEmpty().MaximumLength(40);
            RuleFor(x => x!.City).NotEmpty().MaximumLength(50);
            RuleFor(x => x!.PostalCode).NotEmpty().Matches(@"^\d{1,8}$").MaximumLength(8);
        });
    }
}


public class BuyerInfoDtoValidator : AbstractValidator<ValueObjects.User.Buyer>
{
    public BuyerInfoDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TypeDocument).NotNull();
        RuleFor(x => x.Document).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .SetValidator(new AddressDtoValidator())
            .When(x => x.ShippingAddress != null);  
    }
}

public class PayerInfoDtoValidator : AbstractValidator<ValueObjects.User.Payer?>
{
    public PayerInfoDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.EmailAddress).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.ContactPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TypeDocument).NotNull();
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BillingAddress)
            .NotNull()
            .SetValidator(new AddressDtoValidator())
            .When(x => x.BillingAddress != null);
    }
}

public class CreditCardInfoDtoValidator : AbstractValidator<ValueObjects.Payment.CreditCard?>
{
    public CreditCardInfoDtoValidator()
    {
        RuleFor(x => x!.ExpirationDate).NotEmpty().Length(7).Matches(@"^\d{4}/\d{2}$").WithMessage("Expiration date must be in YYYY/MM format.");
        RuleFor(x => x!.Token).NotEmpty().NotNull();
    }
}

public class PseInfoDtoValidator : AbstractValidator<ValueObjects.Payment.Pse?>
{
    public PseInfoDtoValidator()
    {
        RuleFor(x => x!.PseCode).NotEmpty().MaximumLength(34);
        RuleFor(x => x!.TypePerson).NotEmpty().MaximumLength(2); // "N" o "J"
        RuleFor(x => x!.PseResponseUrl).NotEmpty().MaximumLength(255).Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).WithMessage("Must be a valid URL.");
    }
}

