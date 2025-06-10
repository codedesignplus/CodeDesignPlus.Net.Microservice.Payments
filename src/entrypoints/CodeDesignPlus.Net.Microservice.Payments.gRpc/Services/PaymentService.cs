using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentById;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure;
using Google.Protobuf.WellKnownTypes;

namespace CodeDesignPlus.Net.Microservice.Payments.gRpc.Services;

public class PaymentService(IMediator mediator, IMapper mapper) : Payment.PaymentBase
{
    public async override Task<Empty> Pay(PayRequest request, ServerCallContext context)
    {
        var command = mapper.Map<PayCommand>(request);
        await mediator.Send(command);

        return new Empty { };
    }

    public async override Task<PaymentResponse> GetPayment(GetPaymentRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.InvalidId);

        var payment = await mediator.Send(new GetPaymentByIdQuery(id));

        return mapper.Map<PaymentResponse>(payment);
    }
}