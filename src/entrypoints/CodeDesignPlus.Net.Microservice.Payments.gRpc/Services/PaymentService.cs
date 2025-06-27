using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentById;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure;
using Google.Protobuf.WellKnownTypes;

namespace CodeDesignPlus.Net.Microservice.Payments.gRpc.Services;

public class PaymentService(IMediator mediator, IMapper mapper) : Payment.PaymentBase
{
    public async override Task<Empty> InitiatePayment(InitiatePaymentRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.InvalidId);

        var command = mapper.Map<InitiatePaymentCommand>(request);

        await mediator.Send(command);

        return new Empty();
    }
    
}