using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentMethod.Commands.UpdatePaymentMethod;

[DtoGenerator]
public record UpdatePaymentMethodCommand(Guid Id, PaymentProvider Provider, string Name, string Code, TypePaymentMethod Type, string? Comments, bool IsActive) : IRequest;

public class Validator : AbstractValidator<UpdatePaymentMethodCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(FieldLength.Name);
        RuleFor(x => x.Code).NotEmpty().NotNull().MaximumLength(32);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Comments).NotEmpty().NotNull().MaximumLength(FieldLength.Notes);
    }
}
