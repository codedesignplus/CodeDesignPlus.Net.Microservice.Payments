namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Commands.DeletePaymentProviderConfig;

public record DeletePaymentProviderConfigCommand(Guid Id) : IRequest;

public class Validator : AbstractValidator<DeletePaymentProviderConfigCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
