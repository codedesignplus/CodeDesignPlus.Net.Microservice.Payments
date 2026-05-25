using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Queries.GetAllBeneficiaries;

public record GetAllBeneficiariesQuery(C.Criteria Criteria) : IRequest<Pagination<BeneficiaryDto>>;
