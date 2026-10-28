using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Providers;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;

/// <summary>
/// Starts a payment on the collection account that charges it and answers with the provider's hosted page.
/// </summary>
/// <remarks>
/// The provider is the one of the account, not one the caller picks: the tenant's account was routed by its country
/// when it was created, and the platform's account is configured. A caller that names a provider only confirms it.
/// </remarks>
public class InitiatePaymentCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, IMerchantCredentialsResolver credentialsResolver, IPaymentProviderFactory providers, IOptions<PaymentProvidersOptions> options)
    : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResponseDto>
{
    public async Task<InitiatePaymentResponseDto> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        bool exist;

        if (user.Tenant != Guid.Empty)
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);
        else
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.PaymentAlredyExists);

        var credentials = await credentialsResolver.ForNewPaymentAsync(user.Tenant, request.PlatformAccount, cancellationToken);

        ApplicationGuard.IsTrue(!string.IsNullOrWhiteSpace(request.Provider) && !string.Equals(request.Provider, credentials.Provider, StringComparison.OrdinalIgnoreCase), Errors.PaymentProviderDoesNotMatchAccount);

        var collect = providers.GetCollect(credentials.Provider);

        var payment = PaymentAggregate.Create(
            request.Id,
            request.Module,
            request.ReferenceId,
            Money.FromLong(request.SubTotal.Value, request.SubTotal.Currency),
            Money.FromLong(request.Tax.Value, request.Tax.Currency),
            Money.FromLong(request.Total.Value, request.Total.Currency),
            request.Buyer,
            request.Description,
            collect.Code,
            request.PlatformAccount,
            user.Tenant,
            user.IdUser
        );

        await repository.CreateAsync(payment, cancellationToken);

        var returnUrl = ReturnUrlOf(request, payment.Id);

        var expiresAt = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromTimeSpan(options.Value.CheckoutLifetime));

        var session = await collect.CreateCheckoutAsync(payment, credentials, returnUrl, expiresAt, cancellationToken);

        payment.SetInitiateResponse(new Dictionary<string, string?> { { "redirectUrl", session.RedirectUrl.ToString() } });

        await repository.UpdateAsync(payment, cancellationToken);

        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);

        return new InitiatePaymentResponseDto
        {
            PaymentId = payment.Id,
            Success = true,
            NextAction = NextActionType.Redirect,
            RedirectUrl = session.RedirectUrl.ToString()
        };
    }

    /// <summary>
    /// The page the provider returns to, with the id of the payment in this service.
    /// </summary>
    /// <remarks>
    /// The return page asks this service for the status by that id and never reads what the provider appends (Wompi
    /// adds its transaction id): those parameters are not signed, so they cannot be trusted.
    /// </remarks>
    private Uri ReturnUrlOf(InitiatePaymentCommand request, Guid paymentId)
    {
        var url = string.IsNullOrWhiteSpace(request.ReturnUrl) ? options.Value.DefaultReturnUrl.ToString() : request.ReturnUrl;

        return new Uri(Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(url, "paymentId", paymentId.ToString()));
    }
}
