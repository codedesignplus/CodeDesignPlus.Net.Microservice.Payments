namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.CreatePayment;

[DtoGenerator]
public record CreatePaymentCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<CreatePaymentCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
    }
}
