using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Queries.GetDisbursementRuleById;

public record GetDisbursementRuleByIdQuery(Guid Id) : IRequest<DisbursementRuleDto>;
