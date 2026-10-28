namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.RefreshPaymentStatus;

/// <summary>
/// Asks the provider how a payment in progress ended and applies it, without waiting for its event.
/// </summary>
/// <param name="Id">Id of the payment.</param>
public record RefreshPaymentStatusCommand(Guid Id) : IRequest<PaymentStatusDto>;

public class RefreshPaymentStatusCommandValidator : AbstractValidator<RefreshPaymentStatusCommand>
{
    public RefreshPaymentStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
