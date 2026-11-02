using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.UpdateStatus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.RefreshPaymentStatus;

/// <summary>
/// Asks the provider about a payment that is still in progress.
/// </summary>
/// <remarks>
/// Serves the page the provider returns to, which must not trust the query string the provider appends, and the
/// reconciliation job that recovers events the webhook never received. Both end in the same
/// <see cref="UpdateStatusCommand"/> the webhook uses, so a payment closes the same way whoever learns first.
/// <para>
/// Un cobro cuya página de pago ya venció (<see cref="PaymentAggregate.CheckoutExpiresAt"/>, el vencimiento que se le envió
/// al proveedor, más <see cref="PaymentProvidersOptions.CheckoutExpirationGrace"/>) y del que el proveedor no tiene ninguna transacción se
/// cierra aquí como vencido (pendings/622). El <c>expiration-time</c> de la página limita el inicio del pago: pasado ese
/// momento y sin transacción, ya no puede aparecer ninguna. Antes ese cobro seguía en curso hasta el barrido diario, un día
/// después, y el módulo que lo pidió nunca recibía respuesta: en Las Acacias, el 2-nov-2026, una reserva de parqueadero
/// abrió Wompi a las 20:29, la página venció a las 20:44 y a las 20:50 el cobro seguía iniciado. Un cobro con una
/// transacción pendiente no se toca: esa transacción todavía puede aprobarse.
/// </para>
/// </remarks>
public class RefreshPaymentStatusCommandHandler(IPaymentRepository repository, IUserContext user, IPaymentProviderFactory providers, IMerchantCredentialsResolver credentialsResolver, IMerchantAccountHealth health, IMediator mediator, IPubSub pubsub, IOptions<PaymentProvidersOptions> options, ILogger<RefreshPaymentStatusCommandHandler> logger) : IRequestHandler<RefreshPaymentStatusCommand, PaymentStatusDto>
{
    public async Task<PaymentStatusDto> Handle(RefreshPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var payment = await FindAsync(request.Id, cancellationToken);

        ApplicationGuard.IsNull(payment, Errors.PaymentNotFound);

        // A payment awaiting a retry is still open: the provider may have approved the retry, or its window may have ended
        // (pendings/622).
        if (payment!.Status is not (PaymentStatus.Initiated or PaymentStatus.Pending or PaymentStatus.AwaitingRetry))
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

        if (outcome is null && IsAbandoned(payment))
        {
            return await ExpireAsync(payment, cancellationToken);
        }

        if (outcome is null || !outcome.IsFinal)
        {
            return PaymentStatusDto.From(payment);
        }

        await mediator.Send(new UpdateStatusCommand(payment.Id, outcome.Status, outcome.Raw, outcome.TransactionId, outcome.Amount, outcome.PaymentMethodType, outcome.ReportedFee, outcome.FailureReason, outcome.ProviderMessage, outcome.RetryableUntil), cancellationToken);

        var updated = await FindAsync(request.Id, cancellationToken);

        return PaymentStatusDto.From(updated!);
    }

    /// <summary>
    /// Si la página de pago de un cobro que nunca llegó al proveedor ya dejó de aceptarlo.
    /// </summary>
    private bool IsAbandoned(PaymentAggregate payment)
    {
        if (payment.Status != PaymentStatus.Initiated)
        {
            return false;
        }

        // Sin el vencimiento que se le envió a la página no se estima uno: cerrar es irreversible y un vencimiento adivinado
        // podría cerrar un cobro que el pagador todavía puede completar. Solo queda así un cobro cuya página nunca se creó
        // (falló la llamada al proveedor después de guardarlo); lo cierra el barrido diario de cobros sin respuesta.
        if (payment.CheckoutExpiresAt is null)
        {
            logger.LogWarning("Payment {PaymentId} of tenant {Tenant} is in progress without the expiration of its hosted page; it is not expired as abandoned.", payment.Id, payment.Tenant);

            return false;
        }

        return SystemClock.Instance.GetCurrentInstant() >= payment.CheckoutExpiresAt.Value + Duration.FromTimeSpan(options.Value.CheckoutExpirationGrace);
    }

    private async Task<PaymentStatusDto> ExpireAsync(PaymentAggregate payment, CancellationToken cancellationToken)
    {
        payment.Expire("La página de pago venció sin que se iniciara ninguna transacción.");

        // El cobro antes que su evento (regla 23, §6): el módulo que lo pidió se entera por el mismo evento de un rechazo y
        // cancela lo que lo esperaba.
        await repository.UpdateAsync(payment, cancellationToken);

        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

        logger.LogInformation("Payment {PaymentId} of tenant {Tenant} expired: its hosted page closed with no transaction at {Provider}.", payment.Id, payment.Tenant, payment.Provider);

        return PaymentStatusDto.From(payment);
    }

    private Task<PaymentAggregate> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return user.Tenant != Guid.Empty
            ? repository.FindAsync<PaymentAggregate>(id, user.Tenant, cancellationToken)
            : repository.FindAsync<PaymentAggregate>(id, cancellationToken);
    }
}
