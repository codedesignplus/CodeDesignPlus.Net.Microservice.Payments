using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.PayWithPse;

[DtoGenerator]
public record PayWithPseCommand(Guid Id, Transaction Transaction) : IRequest;

public class Validator : AbstractValidator<PayWithPseCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Transaction)
            .NotNull()
            .WithMessage("Transaction cannot be null.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Transaction.Order)
                    .NotNull()
                    .WithMessage("Transaction order cannot be null.")
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.Transaction.Order.Description)
                            .NotEmpty()
                            .NotNull()
                            .MinimumLength(1)
                            .MaximumLength(255)
                            .WithMessage("Order description cannot be empty or null and must be between 1 and 255 characters long.");

                        RuleFor(x => x.Transaction.Order.Buyer)
                            .NotNull()
                            .WithMessage("Order buyer cannot be null.")
                            .DependentRules(() =>
                            {
                                RuleFor(x => x.Transaction.Order.Buyer.FullName)
                                    .NotEmpty()
                                    .NotNull()
                                    .MinimumLength(1)
                                    .MaximumLength(150)
                                    .WithMessage("Buyer full name cannot be empty or null and must be between 1 and 150 characters long.");

                                RuleFor(x => x.Transaction.Order.Buyer.EmailAddress)
                                    .NotEmpty()
                                    .NotNull()
                                    .EmailAddress()
                                    .MinimumLength(1)
                                    .MaximumLength(255)
                                    .WithMessage("Buyer email address cannot be empty or null, must be a valid email, and must be between 1 and 255 characters long.");

                                RuleFor(x => x.Transaction.Order.Buyer.ContactPhone)
                                    .NotEmpty()
                                    .NotNull()
                                    .MinimumLength(1)
                                    .MaximumLength(20)
                                    .WithMessage("Buyer contact phone cannot be empty or null and must be between 1 and 20 characters long.");

                                RuleFor(x => x.Transaction.Order.Buyer.DniNumber)
                                    .NotEmpty()
                                    .NotNull()
                                    .MaximumLength(20)
                                    .WithMessage("Buyer DNI number cannot be empty or null and must be up to 20 characters long.");

                                RuleFor(x => x.Transaction.Order.Buyer.ShippingAddress)
                                    .NotNull()
                                    .WithMessage("Buyer shipping address cannot be null.");
                            });

                    });

                RuleFor(x => x.Transaction.Payer)
                    .NotNull()
                    .WithMessage("Payer cannot be null.")
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.Transaction.Payer.FullName)
                            .NotEmpty()
                            .NotNull()
                            .MinimumLength(1)
                            .MaximumLength(150)
                            .WithMessage("Payer full name cannot be empty or null and must be between 1 and 150 characters long.");

                        RuleFor(x => x.Transaction.Payer.EmailAddress)
                            .NotEmpty()
                            .NotNull()
                            .EmailAddress()
                            .MinimumLength(1)
                            .MaximumLength(255)
                            .WithMessage("Payer email address cannot be empty or null, must be a valid email, and must be between 1 and 255 characters long.");

                        RuleFor(x => x.Transaction.Payer.ContactPhone)
                            .NotEmpty()
                            .NotNull()
                            .MinimumLength(1)
                            .MaximumLength(20)
                            .WithMessage("Payer contact phone cannot be empty or null and must be between 1 and 20 characters long.");

                        RuleFor(x => x.Transaction.Payer.DniNumber)
                            .NotEmpty()
                            .NotNull()
                            .MaximumLength(20)
                            .WithMessage("Payer DNI number cannot be empty or null and must be up to 20 characters long.");

                        RuleFor(x => x.Transaction.Payer.BillingAddress)
                            .NotNull()
                            .WithMessage("Payer billing address cannot be null.")
                            .DependentRules(() =>
                            {
                                RuleFor(x => x.Transaction.Payer.BillingAddress.Street)
                                    .NotEmpty()
                                    .NotNull()
                                    .MinimumLength(1)
                                    .MaximumLength(100)
                                    .WithMessage("Payer billing address street cannot be empty or null and must be between 1 and 100 characters long.");

                                RuleFor(x => x.Transaction.Payer.BillingAddress.Country)
                                    .NotEmpty()
                                    .NotNull()
                                    .MinimumLength(1)
                                    .MaximumLength(2)
                                    .Matches(@"^[A-Z]{2}$")
                                    .WithMessage("Payer billing address country cannot be empty or null, must be a two-letter uppercase ISO 3166-1 alpha-2 code, and must be exactly 2 characters long.");

                                RuleFor(x => x.Transaction.Payer.BillingAddress.State)
                                    .MinimumLength(1)
                                    .MaximumLength(40)
                                    .WithMessage("Payer billing address state cannot be empty or null and must be between 1 and 40 characters long.");

                                RuleFor(x => x.Transaction.Payer.BillingAddress.City)
                                    .NotEmpty()
                                    .NotNull()
                                    .MinimumLength(1)
                                    .MaximumLength(50)
                                    .WithMessage("Payer billing address city cannot be empty or null and must be between 1 and 50 characters long.");

                                RuleFor(x => x.Transaction.Payer.BillingAddress.PostalCode)
                                    .Matches(@"^\d{1,8}$")
                                    .MinimumLength(1)
                                    .MaximumLength(8)
                                    .WithMessage("Payer billing address postal code cannot be empty or null and must be numeric and up to 8 digits long.");

                                RuleFor(x => x.Transaction.Payer.BillingAddress.Phone)
                                    .MinimumLength(1)
                                    .MaximumLength(11)
                                    .Matches(@"^\+?\d{1,11}$")
                                    .WithMessage("Payer billing address phone cannot be empty or null, must be numeric, and can include a leading + sign, with a maximum of 11 characters.");
                            });
                    });

                RuleFor(x => x.Transaction.PaymentMethod)
                    .NotEmpty()
                    .NotNull()
                    .MaximumLength(32)
                    .WithMessage("Transaction payment method cannot be empty or null and must be up to 32 characters long.");

                // RuleFor(x => x.Transaction.CreditCard)
                //     .NotNull()
                //     .WithMessage("Transaction credit card cannot be null.")
                //     .DependentRules(() =>
                //     {
                //         RuleFor(x => x.Transaction.CreditCard.Number)
                //             .NotEmpty()
                //             .NotNull()
                //             .MinimumLength(13)
                //             .MaximumLength(20)
                //             .WithMessage("Credit card number cannot be empty or null and must be between 13 and 20 characters long.");

                //         RuleFor(x => x.Transaction.CreditCard.SecurityCode)
                //             .NotEmpty()
                //             .NotNull()
                //             .MinimumLength(1)
                //             .MaximumLength(4)
                //             .WithMessage("Credit card security code cannot be empty or null and must be between 1 and 4 characters long.");

                //         RuleFor(x => x.Transaction.CreditCard.ExpirationDate)
                //             .NotEmpty()
                //             .NotNull()
                //             .Matches(@"^\d{4}/\d{2}$")
                //             .MinimumLength(7)
                //             .MaximumLength(7)
                //             .WithMessage("Credit card expiration date cannot be empty or null, must be in YYYY/MM format, and exactly 7 characters long.");

                //         RuleFor(x => x.Transaction.CreditCard.Name)
                //             .NotEmpty()
                //             .NotNull()
                //             .MinimumLength(1)
                //             .MaximumLength(255)
                //             .WithMessage("Credit card name cannot be empty or null and must be between 1 and 255 characters long.");
                //     });
            });
    }
}
