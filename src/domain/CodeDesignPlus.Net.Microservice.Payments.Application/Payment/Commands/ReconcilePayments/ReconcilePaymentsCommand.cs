namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ReconcilePayments;

/// <summary>
/// Asks the provider about the payments still in progress, in case their event was lost.
/// </summary>
public record ReconcilePaymentsCommand : IRequest;
