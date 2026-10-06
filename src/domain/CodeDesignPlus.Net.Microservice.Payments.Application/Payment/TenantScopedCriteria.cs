namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment;

/// <summary>
/// Acota el filtro que manda la pantalla a los cobros de una copropiedad (pendings/295).
/// </summary>
/// <remarks>
/// <see cref="Domain.PaymentAggregate"/> hereda de <c>AggregateRootBase</c>, así que el repositorio del SDK no le
/// aplica el filtro de tenant aunque se le pase: el listado servía los cobros de todas las copropiedades. El tenant se
/// añade aquí al filtro. El parser de criteria agrupa de izquierda a derecha y no admite paréntesis, así que va al
/// final: <c>x|or|y|and|tenant=A</c> se lee <c>(x o y) y tenant A</c>, y un <c>|or|</c> del cliente no se lo salta.
/// Los cobros de plataforma, que no llevan tenant, nunca cumplen la condición.
/// </remarks>
public static class TenantScopedCriteria
{
    /// <summary>
    /// Devuelve una copia de <paramref name="criteria"/> cuyo filtro solo deja pasar los cobros de la copropiedad.
    /// </summary>
    /// <param name="tenant">La copropiedad en sesión; no puede estar vacía.</param>
    /// <param name="criteria">El criterio que mandó la pantalla.</param>
    /// <returns>El criterio acotado, con la misma paginación y el mismo orden.</returns>
    public static C.Criteria Scope(Guid tenant, C.Criteria criteria)
    {
        ApplicationGuard.IsNull(criteria, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(tenant, Errors.TenantIsRequiredToListPayments);

        var clientFilters = criteria.Filters?.Trim() ?? string.Empty;
        var tenantFilter = $"tenant={tenant}";

        return new C.Criteria
        {
            Filters = string.IsNullOrEmpty(clientFilters) ? tenantFilter : $"{clientFilters}|and|{tenantFilter}",
            OrderBy = criteria.OrderBy,
            OrderType = criteria.OrderType,
            Skip = criteria.Skip,
            Limit = criteria.Limit,
        };
    }
}
