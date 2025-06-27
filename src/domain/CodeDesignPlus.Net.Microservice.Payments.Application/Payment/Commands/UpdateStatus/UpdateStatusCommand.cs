namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;

[DtoGenerator]
public record UpdateStatusCommand(Guid Id) : IRequest<PaymentResponseDto>;

public class Validator : AbstractValidator<UpdateStatusCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
    }
}
