namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ProcessProviderEvent;

/// <summary>
/// An event a provider posted to the webhook, still untrusted.
/// </summary>
/// <param name="Provider">Provider code from the webhook URL.</param>
/// <param name="Body">The raw body, exactly as it arrived: its signature is computed over it.</param>
public record ProcessProviderEventCommand(string Provider, string Body) : IRequest;

public class ProcessProviderEventCommandValidator : AbstractValidator<ProcessProviderEventCommand>
{
    public ProcessProviderEventCommandValidator()
    {
        RuleFor(x => x.Provider).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Body).NotEmpty();
    }
}
