using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.DisbursementRule.Queries.GetAllDisbursementRules;

public record GetAllDisbursementRulesQuery(C.Criteria Criteria) : IRequest<Pagination<DisbursementRuleDto>>;
