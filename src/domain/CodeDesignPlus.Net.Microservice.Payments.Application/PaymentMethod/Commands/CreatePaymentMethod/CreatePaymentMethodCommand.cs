using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentMethod.Commands.CreatePaymentMethod;

[DtoGenerator]
public record CreatePaymentMethodCommand(Guid Id, PaymentProvider Provider, string Name, string Code, TypePaymentMethod Type, string? Comments) : IRequest;

public class Validator : AbstractValidator<CreatePaymentMethodCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(64);
        RuleFor(x => x.Code).NotEmpty().NotNull().MaximumLength(32);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Comments).NotEmpty().NotNull().MaximumLength(124);
    }
}
