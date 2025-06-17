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

        logger.LogInformation("Processing payment for Order ID: {OrderId}", request.Id);

        var command = mapper.Map<PayCommand>(request);
        await mediator.Send(command);


        var payment = await mediator.Send(new GetPaymentByIdQuery(id));

        return mapper.Map<PaymentResponse>(payment);
    }

    public async override Task<PaymentResponse> GetPayment(GetPaymentRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.InvalidId);

        var payment = await mediator.Send(new GetPaymentByIdQuery(id));

        return mapper.Map<PaymentResponse>(payment);
    }
}