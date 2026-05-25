using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Queries.GetBeneficiaryById;

public record GetBeneficiaryByIdQuery(Guid Id) : IRequest<BeneficiaryDto>;
