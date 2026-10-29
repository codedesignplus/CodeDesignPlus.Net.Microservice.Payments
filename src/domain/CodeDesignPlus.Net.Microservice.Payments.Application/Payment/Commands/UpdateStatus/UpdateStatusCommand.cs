using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;

/// <summary>
/// Closes a payment with the outcome the provider reported.
/// </summary>
/// <param name="Id">Id of the payment.</param>
/// <param name="Status">The final status.</param>
/// <param name="Metadata">What the provider reported.</param>
/// <param name="ProviderTransactionId">Id of the transaction at the provider.</param>
/// <param name="CapturedAmount">What the provider captured.</param>
/// <param name="PaymentMethodType">Payment method the payer chose on the provider's page.</param>
/// <param name="ReportedFee">The commission the provider reported for the transaction, when it reports it.</param>
public record UpdateStatusCommand(
    Guid Id,
    PaymentStatus Status,
    Dictionary<string, string?> Metadata,
    string? ProviderTransactionId = null,
    Money? CapturedAmount = null,
    string? PaymentMethodType = null,
    ProviderFee? ReportedFee = null
) : IRequest;

public class Validator : AbstractValidator<UpdateStatusCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().NotNull();
        RuleFor(x => x.Metadata).NotNull().NotEmpty();
    }
}
