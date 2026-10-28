using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Queries.GetMerchantAccount;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentStatus;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payout.Commands.RequestPayout;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payout.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payout.Queries.GetPayout;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;

namespace CodeDesignPlus.Net.Microservice.Payments.gRpc.Services;

/// <summary>
/// The payments contract other services call inside the cluster.
/// </summary>
/// <remarks>
/// <c>GetMerchantAccount</c>, <c>RequestPayout</c> and <c>GetPayout</c> are anonymous because a worker calls them without
/// a user token, so the tenant travels in the request. That is only safe because this gRPC endpoint is not published
/// outside the cluster: its Service is a ClusterIP and no VirtualService or Ingress routes to it (checked on 28-oct-2026
/// in the Helm values and in the live cluster, pendings/660). Publishing it would expose these three methods.
/// </remarks>
public class PaymentService(IMediator mediator, IMapper mapper) : Payment.PaymentBase
{
    public override async Task<InitiatePaymentResponse> InitiatePayment(InitiatePaymentRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out _), Errors.InvalidId);

        var command = mapper.Map<InitiatePaymentCommand>(request);

        try
        {
            var response = await mediator.Send(command, context.CancellationToken);

            return mapper.Map<InitiatePaymentResponse>(response);
        }
        catch (CodeDesignPlusException exception) when (TransientFailures.IsTransient(exception))
        {
            // Unavailable, not FailedPrecondition: the module can offer the payment again in a moment.
            throw new RpcException(new Status(StatusCode.Unavailable, $"{exception.Code}: {exception.Message}"));
        }
    }

    public override async Task<GetPaymentStatusResponse> GetPaymentStatus(GetPaymentStatusRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.InvalidId);

        var payment = await mediator.Send(new GetPaymentStatusQuery(id), context.CancellationToken);

        var response = new GetPaymentStatusResponse
        {
            Status = (PaymentStatus)(int)payment.Status,
            Provider = payment.Provider
        };

        if (payment.CapturedAmount is not null)
        {
            response.CapturedAmount = ToAmount(payment.CapturedAmount);
        }

        return response;
    }

    [AllowAnonymous]
    public override async Task<GetMerchantAccountResponse> GetMerchantAccount(GetMerchantAccountRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Tenant, out var tenant), Errors.InvalidId);

        var account = await mediator.Send(new GetMerchantAccountQuery(tenant), context.CancellationToken);

        var response = new GetMerchantAccountResponse
        {
            Id = account.Id.ToString(),
            Tenant = account.Tenant.ToString(),
            Provider = account.Provider ?? string.Empty,
            Environment = account.Environment is { } environment ? (MerchantAccountEnvironment)(int)environment : MerchantAccountEnvironment.Unspecified,
            Status = (MerchantAccountStatus)(int)account.Status,
            PayoutsEnabled = account.PayoutsEnabled
        };

        if (account.LastValidatedAt is { } lastValidatedAt)
        {
            response.LastValidatedAt = Timestamp.FromDateTimeOffset(lastValidatedAt.ToDateTimeOffset());
        }

        return response;
    }

    [AllowAnonymous]
    public override async Task<RequestPayoutResponse> RequestPayout(RequestPayoutRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.InvalidId);
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Tenant, out var tenant), Errors.InvalidId);
        InfrastructureGuard.IsFalse(Guid.TryParse(request.ReferenceId, out var referenceId), Errors.InvalidId);
        InfrastructureGuard.IsFalse(Guid.TryParse(request.BeneficiaryId, out var beneficiaryId), Errors.InvalidId);
        InfrastructureGuard.IsFalse(Guid.TryParse(request.RequestedBy, out var requestedBy), Errors.InvalidId);
        InfrastructureGuard.IsNull(request.Amount, Errors.InvalidId);

        var command = new RequestPayoutCommand(
            id,
            tenant,
            request.Module,
            referenceId,
            (Domain.Enums.PayoutPurpose)(int)request.Purpose,
            beneficiaryId,
            Money.FromLong(request.Amount.Value, request.Amount.Currency),
            request.Description,
            requestedBy);

        var payout = await mediator.Send(command, context.CancellationToken);

        return new RequestPayoutResponse
        {
            PayoutId = payout.Id.ToString(),
            Status = (PayoutStatus)(int)payout.Status
        };
    }

    [AllowAnonymous]
    public override async Task<GetPayoutResponse> GetPayout(GetPayoutRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.InvalidId);
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Tenant, out var tenant), Errors.InvalidId);

        var payout = await mediator.Send(new GetPayoutQuery(id, tenant), context.CancellationToken);

        return ToResponse(payout);
    }

    private static GetPayoutResponse ToResponse(PayoutDto payout)
    {
        var response = new GetPayoutResponse
        {
            Id = payout.Id.ToString(),
            Module = payout.Module,
            ReferenceId = payout.ReferenceId.ToString(),
            Purpose = (PayoutPurpose)(int)payout.Purpose,
            Amount = ToAmount(payout.Amount),
            Status = (PayoutStatus)(int)payout.Status
        };

        if (payout.Provider is not null)
        {
            response.Provider = payout.Provider;
        }

        if (payout.ProviderBatchId is not null)
        {
            response.ProviderBatchId = payout.ProviderBatchId;
        }

        if (payout.ProviderTransactionId is not null)
        {
            response.ProviderTransactionId = payout.ProviderTransactionId;
        }

        if (payout.FailureReason is not null)
        {
            response.FailureReason = payout.FailureReason;
        }

        return response;
    }

    private static Amount ToAmount(Money money) => new() { Value = money.Amount, Currency = money.Currency };
}
