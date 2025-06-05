namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentById;

public class GetPaymentByIdQueryHandler(IPaymentRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
{
    public Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult<PaymentDto>(default!);
    }
}
