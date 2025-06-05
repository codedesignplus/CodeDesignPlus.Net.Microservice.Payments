namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdatePayment;

[DtoGenerator]
public record UpdatePaymentCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<UpdatePaymentCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
    }
}
