using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.RefreshPaymentStatus;

/// <summary>
/// Asks the provider about a payment that is still in progress.
/// </summary>
/// <remarks>
/// Serves the page the provider returns to, which must not trust the query string the provider appends, and the
/// reconciliation job that recovers events the webhook never received. Both end in the same
/// <see cref="UpdateStatusCommand"/> the webhook uses, so a payment closes the same way whoever learns first.
/// </remarks>
public class RefreshPaymentStatusCommandHandler(IPaymentRepository repository, IUserContext user, IPaymentProviderFactory providers, IMerchantCredentialsResolver credentialsResolver, IMerchantAccountHealth health, IMediator mediator, ILogger<RefreshPaymentStatusCommandHandler> logger) : IRequestHandler<RefreshPaymentStatusCommand, PaymentStatusDto>
{
    public async Task<PaymentStatusDto> Handle(RefreshPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var payment = await FindAsync(request.Id, cancellationToken);

        ApplicationGuard.IsNull(payment, Errors.PaymentNotFound);

        if (payment!.Status is not (PaymentStatus.Initiated or PaymentStatus.Pending))
        {
            return PaymentStatusDto.From(payment);
        }

        var credentials = await credentialsResolver.ForPaymentAsync(payment, cancellationToken);

        if (credentials is null)
        {
            logger.LogWarning("Payment {PaymentId} of tenant {Tenant} cannot be refreshed: its collection account has no keys.", payment.Id, payment.Tenant);

            return PaymentStatusDto.From(payment);
        }

        CollectOutcome? outcome;

        try
        {
            outcome = await providers.GetCollect(payment.Provider).GetOutcomeAsync(payment, credentials, cancellationToken);
        }
        catch (CodeDesignPlusException exception) when (health.IsRejection(exception))
        {
            // The status stays as it was; the account counts the rejection and is invalidated if it repeats.
            await health.ReportRejectionAsync(payment, Errors.MerchantPrivateKeyWasRejected, cancellationToken);

            return PaymentStatusDto.From(payment);
        }

        await health.ReportAcceptanceAsync(payment, credentials, cancellationToken);

        if (outcome is null || !outcome.IsFinal)
        {
            return PaymentStatusDto.From(payment);
        }

        await mediator.Send(new UpdateStatusCommand(payment.Id, outcome.Status, outcome.Raw, outcome.TransactionId, outcome.Amount, outcome.PaymentMethodType), cancellationToken);

        var updated = await FindAsync(request.Id, cancellationToken);

        return PaymentStatusDto.From(updated!);
    }

    private Task<PaymentAggregate> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return user.Tenant != Guid.Empty
            ? repository.FindAsync<PaymentAggregate>(id, user.Tenant, cancellationToken)
            : repository.FindAsync<PaymentAggregate>(id, cancellationToken);
    }
}
