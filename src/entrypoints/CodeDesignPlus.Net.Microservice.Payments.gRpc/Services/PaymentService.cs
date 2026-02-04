using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure;
using Google.Protobuf.WellKnownTypes;

namespace CodeDesignPlus.Net.Microservice.Payments.gRpc.Services;

public class PaymentService(IMediator mediator, IMapper mapper) : Payment.PaymentBase
{
    public override async Task<InitiatePaymentResponse> InitiatePayment(InitiatePaymentRequest request, ServerCallContext context)
    {
        InfrastructureGuard.IsFalse(Guid.TryParse(request.Id, out var id), Errors.InvalidId);

        var command = mapper.Map<InitiatePaymentCommand>(request);

        var response = await mediator.Send(command);

        var result = mapper.Map<InitiatePaymentResponse>(response);

        if (response.WidgetParameters != null)
            foreach (var item in response.WidgetParameters.Where(x => x.Value != null))
                result.Metadata.Add(item.Key, item.Value);

        return result;
    }
}