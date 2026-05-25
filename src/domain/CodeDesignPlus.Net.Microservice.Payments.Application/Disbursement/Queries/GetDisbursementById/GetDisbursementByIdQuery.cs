using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Queries.GetDisbursementById;

public record GetDisbursementByIdQuery(Guid Id) : IRequest<DisbursementDto>;
