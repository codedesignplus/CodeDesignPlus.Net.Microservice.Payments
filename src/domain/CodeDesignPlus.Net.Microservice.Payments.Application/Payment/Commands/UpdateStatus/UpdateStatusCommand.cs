using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;

[DtoGenerator]
public record UpdateStatusCommand(
    Guid Id,
    PaymentStatus Status,
    Dictionary<string, string> Metadata
) : IRequest;

public class Validator : AbstractValidator<UpdateStatusCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Metadata).NotNull().NotEmpty();
    }
}
