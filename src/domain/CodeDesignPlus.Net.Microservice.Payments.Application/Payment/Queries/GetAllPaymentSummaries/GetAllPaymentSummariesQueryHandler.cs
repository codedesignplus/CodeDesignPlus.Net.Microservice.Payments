using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPaymentSummaries;

/// <summary>
/// Manejador de <see cref="GetAllPaymentSummariesQuery"/>.
/// </summary>
/// <remarks>
/// El filtrado por tenant no es cosmetico en este agregado: el pago lleva el nombre del comprador y el
/// importe que se le cobro, asi que servirle a una copropiedad los cobros de otra seria una fuga en si
/// misma, aunque ningun campo sensible salga en el DTO.
/// </remarks>
public class GetAllPaymentSummariesQueryHandler(IPaymentRepository repository, IMapper mapper, IUserContext user) : IRequestHandler<GetAllPaymentSummariesQuery, Pagination<PaymentSummaryDto>>
{
    /// <summary>
    /// Resuelve el listado paginado de cobros visibles para el administrador.
    /// </summary>
    /// <param name="request">La consulta con los criterios de busqueda.</param>
    /// <param name="cancellationToken">Token para monitorear solicitudes de cancelacion.</param>
    /// <returns>La pagina de cobros, sin datos de tarjeta ni del documento del comprador.</returns>
    public async Task<Pagination<PaymentSummaryDto>> Handle(GetAllPaymentSummariesQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var payments = await repository.MatchingAsync<PaymentAggregate>(request.Criteria, user.Tenant, cancellationToken);

        return mapper.Map<Pagination<PaymentSummaryDto>>(payments);
    }
}
