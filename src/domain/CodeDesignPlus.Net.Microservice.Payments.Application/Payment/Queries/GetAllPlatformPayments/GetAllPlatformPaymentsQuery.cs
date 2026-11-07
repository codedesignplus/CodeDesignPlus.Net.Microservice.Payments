using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPlatformPayments;

/// <summary>
/// The page of payments charged to the platform's own merchant account, for the Super Admin (pendings/674).
/// </summary>
/// <param name="Criteria">Filters, order and page sent by the screen.</param>
public record GetAllPlatformPaymentsQuery(C.Criteria Criteria) : IRequest<Pagination<PlatformPaymentDto>>;
