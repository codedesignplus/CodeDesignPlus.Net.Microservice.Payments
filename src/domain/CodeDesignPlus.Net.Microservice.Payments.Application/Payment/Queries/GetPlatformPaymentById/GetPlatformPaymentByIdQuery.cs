namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPlatformPaymentById;

/// <summary>
/// One payment charged to the platform's own merchant account, for the detail of a license order (pendings/674).
/// </summary>
/// <param name="Id">Id of the payment: the <c>paymentId</c> of the order in ms-licenses.</param>
public record GetPlatformPaymentByIdQuery(Guid Id) : IRequest<PlatformPaymentDto>;
