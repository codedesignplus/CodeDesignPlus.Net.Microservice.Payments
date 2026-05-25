using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Queries.GetAllDisbursements;

public record GetAllDisbursementsQuery(C.Criteria Criteria) : IRequest<Pagination<DisbursementDto>>;
