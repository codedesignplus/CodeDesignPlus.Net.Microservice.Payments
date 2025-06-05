namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentById;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentDto>;

