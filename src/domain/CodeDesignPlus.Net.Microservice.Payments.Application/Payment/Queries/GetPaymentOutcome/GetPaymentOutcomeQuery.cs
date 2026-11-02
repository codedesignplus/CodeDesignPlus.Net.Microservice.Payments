namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentOutcome;

/// <summary>
/// How a payment ended and why, for the payer's screen (pendings/622).
/// </summary>
/// <param name="Id">Id of the payment.</param>
public record GetPaymentOutcomeQuery(Guid Id) : IRequest<PaymentOutcomeDto>;
