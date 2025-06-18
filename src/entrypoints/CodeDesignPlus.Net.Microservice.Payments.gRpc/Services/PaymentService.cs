using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentById;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure;
using Google.Protobuf.WellKnownTypes;

namespace CodeDesignPlus.Net.Microservice.Payments.gRpc.Services;

public class PaymentService(IMediator mediator, IMapper mapper, ILogger<PaymentService> logger) : Payment.PaymentBase
{
    public async override Task<PaymentResponse> Pay(PayRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.InvalidId);

        logger.LogWarning("Processing payment for Order ID: {OrderId}", request.Id);

        var command = mapper.Map<PayCommand>(request);

        await mediator.Send(command);

        logger.LogWarning("Payment processed successfully for Order ID: {OrderId}", request.Id);

        var payment = await mediator.Send(new GetPaymentByIdQuery(id));

        logger.LogWarning("Retrieved payment details for Order ID: {OrderId}, data: {@Payment}", request.Id, payment);

        var dto = mapper.Map<PaymentResponse>(payment);

        logger.LogWarning("Mapped payment details to DTO for Order ID: {OrderId}, data: {@Dto}", request.Id, dto);

        SetExtraInfo(payment, dto);

        logger.LogWarning("Set extra info for payment response for Order ID: {OrderId}", request.Id);

        return dto;
    }


    public async override Task<PaymentResponse> GetPayment(GetPaymentRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.InvalidId);

        var payment = await mediator.Send(new GetPaymentByIdQuery(id));

        var dto = mapper.Map<PaymentResponse>(payment);
        
        SetExtraInfo(payment, dto);

        return dto;
    }

    
    private static void SetExtraInfo(PaymentDto payment, PaymentResponse dto)
    {
        foreach (var item in payment.Response.TransactionResponse.AdditionalInfo)
        {
            dto.Response.TransactionResponse.AdditionalData.Add(item.Key, item.Value ?? string.Empty);
        }

        foreach (var item in payment.Response.TransactionResponse.ExtraParameters)
        {
            dto.Response.TransactionResponse.ExtraParameters.Add(item.Key, item.Value ?? string.Empty);
        }
    }

    
}