using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPaymentSummaries;

/// <summary>
/// Listado paginado de cobros para la pantalla de consulta del administrador.
/// </summary>
/// <remarks>
/// Es la version publicable de <c>GetAllPaymentQuery</c>: misma busqueda, otro contrato de salida. Aquella
/// devuelve <c>PaymentDto</c>, que lleva el token de la tarjeta, el titular, el codigo de seguridad y el
/// documento del comprador, y por eso no sale de la capa de aplicacion.
/// </remarks>
/// <param name="Criteria">Filtros, orden y pagina que llegan del cliente.</param>
public record GetAllPaymentSummariesQuery(C.Criteria Criteria) : IRequest<Pagination<PaymentSummaryDto>>;
