using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Queries.GetPaymentProviderConfigById;

public record GetPaymentProviderConfigByIdQuery(Guid Id) : IRequest<PaymentProviderConfigDto>;
