using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Queries.GetDisbursementsByPayment;

public record GetDisbursementsByPaymentQuery(Guid PaymentId) : IRequest<List<DisbursementDto>>;
