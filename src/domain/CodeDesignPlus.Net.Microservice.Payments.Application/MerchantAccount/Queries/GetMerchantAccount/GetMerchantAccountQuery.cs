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

public class GetMerchantAccountQueryHandler(IMerchantAccountRepository repository, IOptions<PaymentProvidersOptions> options) : IRequestHandler<GetMerchantAccountQuery, MerchantAccountDto>
{
    public async Task<MerchantAccountDto> Handle(GetMerchantAccountQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);
        ApplicationGuard.GuidIsEmpty(request.Tenant, Errors.TenantIsRequired);

        var account = await repository.GetByTenantAsync(request.Tenant, cancellationToken);

        return account is null
            ? MerchantAccountDto.Pending(request.Tenant, null, options.Value.EventsUrl)
            : MerchantAccountDto.From(account, options.Value.EventsUrl);
    }
}
