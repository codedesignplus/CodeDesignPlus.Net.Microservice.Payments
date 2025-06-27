using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentMethod.Queries.GetPaymentMethodsByProvider;

public record GetPaymentMethodsByProviderQuery(PaymentProvider Provider, List<TypePaymentMethod> Methods) : IRequest<List<PaymentMethodDto>>;

