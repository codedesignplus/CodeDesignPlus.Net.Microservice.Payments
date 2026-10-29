using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Queries.GetMerchantAccount;

/// <summary>
/// The collection account of a tenant. A tenant without one is answered as waiting for its keys.
/// </summary>
/// <param name="Tenant">The tenant.</param>
public record GetMerchantAccountQuery(Guid Tenant) : IRequest<MerchantAccountDto>;

public class GetMerchantAccountQueryValidator : AbstractValidator<GetMerchantAccountQuery>
{
    public GetMerchantAccountQueryValidator()
    {
        RuleFor(x => x.Tenant).NotEmpty();
    }
}

/// <summary>
/// Answers a tenant without an account with the provider its country is routed to, the same one the account would get
/// when created: an empty provider then means only that no provider serves the country, not that the tenant predates
/// collection accounts.
/// </summary>
/// <remarks>
/// The country comes from the tenant context the security middleware loads from the tenant directory. A call without
/// that tenant in its header (a worker over gRPC) loads it here from the same directory.
/// </remarks>
public class GetMerchantAccountQueryHandler(IMerchantAccountRepository repository, IUserContext user, Security.Abstractions.ITenant tenantContext, IOptions<PaymentProvidersOptions> options) : IRequestHandler<GetMerchantAccountQuery, MerchantAccountDto>
{
    public async Task<MerchantAccountDto> Handle(GetMerchantAccountQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(request.Tenant, Errors.TenantIsRequired);

        var account = await repository.GetByTenantAsync(request.Tenant, cancellationToken);

        if (account is not null)
        {
            return MerchantAccountDto.From(account, options.Value.EventsUrl);
        }

        var country = await CountryOfAsync(request.Tenant, cancellationToken);

        return MerchantAccountDto.Pending(request.Tenant, options.Value.ProviderFor(country), options.Value.EventsUrl);
    }

    private async Task<string?> CountryOfAsync(Guid tenant, CancellationToken cancellationToken)
    {
        if (tenantContext.IsLoaded && user.Tenant == tenant)
        {
            return tenantContext.Country?.Alpha2;
        }

        if (tenantContext.IsLoaded)
        {
            // Loaded for another tenant than the one asked about: overwriting it would change the context of the request.
            return null;
        }

        await tenantContext.SetAsync(tenant, cancellationToken);

        return tenantContext.Country?.Alpha2;
    }
}
