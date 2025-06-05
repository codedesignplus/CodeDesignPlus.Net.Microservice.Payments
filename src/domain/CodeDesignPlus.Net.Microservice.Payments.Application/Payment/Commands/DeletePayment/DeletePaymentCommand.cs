namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.DeletePayment;

[DtoGenerator]
public record DeletePaymentCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<DeletePaymentCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
    }
}
