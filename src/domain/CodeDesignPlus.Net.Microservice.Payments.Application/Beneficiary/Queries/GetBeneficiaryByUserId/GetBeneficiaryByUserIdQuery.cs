using CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Queries.GetBeneficiaryByUserId;

public record GetBeneficiaryByUserIdQuery(Guid UserId) : IRequest<BeneficiaryDto>;
