namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment;

/// <summary>
/// Narrows the filter the screen sends to the payments charged to the platform's own merchant account (pendings/674).
/// </summary>
/// <remarks>
/// Same shape as <see cref="TenantScopedCriteria"/>: the criteria parser groups left to right and has no parentheses, so the
/// condition goes last and an <c>|or|</c> of the client cannot skip it. A tenant's payments never meet it.
/// </remarks>
public static class PlatformScopedCriteria
{
    /// <summary>
    /// Returns a copy of <paramref name="criteria"/> whose filter only lets through the platform's payments.
    /// </summary>
    /// <param name="criteria">The criteria the screen sent.</param>
    /// <returns>The narrowed criteria, with the same page and order.</returns>
    public static C.Criteria Scope(C.Criteria criteria)
    {
        ApplicationGuard.IsNull(criteria, Errors.InvalidRequest);

        var clientFilters = criteria.Filters?.Trim() ?? string.Empty;
        var platformFilter = $"{nameof(PaymentAggregate.PlatformAccount)}=true";

        return new C.Criteria
        {
            Filters = string.IsNullOrEmpty(clientFilters) ? platformFilter : $"{clientFilters}|and|{platformFilter}",
            OrderBy = criteria.OrderBy,
            OrderType = criteria.OrderType,
            Skip = criteria.Skip,
            Limit = criteria.Limit,
        };
    }
}
