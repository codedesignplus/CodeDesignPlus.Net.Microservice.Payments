namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPayment;

public record GetAllPaymentQuery(Guid Id) : IRequest<PaymentDto>;

