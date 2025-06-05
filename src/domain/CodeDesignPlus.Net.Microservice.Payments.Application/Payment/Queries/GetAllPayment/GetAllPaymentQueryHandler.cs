namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPayment;

public class GetAllPaymentQueryHandler(IPaymentRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetAllPaymentQuery, PaymentDto>
{
    public Task<PaymentDto> Handle(GetAllPaymentQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult<PaymentDto>(default!);
    }
}
