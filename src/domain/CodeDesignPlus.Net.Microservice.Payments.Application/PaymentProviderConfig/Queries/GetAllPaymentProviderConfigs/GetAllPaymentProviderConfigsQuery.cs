using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Queries.GetAllPaymentProviderConfigs;

public record GetAllPaymentProviderConfigsQuery(C.Criteria Criteria) : IRequest<Pagination<PaymentProviderConfigDto>>;
