using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPayment;

public record GetAllPaymentQuery(C.Criteria Criteria) : IRequest<Pagination<PaymentDto>>;

