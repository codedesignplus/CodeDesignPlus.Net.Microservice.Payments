using CodeDesignPlus.Net.Microservice.Payments.Application.Payout.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payout.Queries.GetPayout;

/// <summary>
/// A payout of a tenant.
/// </summary>
/// <param name="Id">Id of the payout.</param>
/// <param name="Tenant">Tenant whose collection account pays it.</param>
public record GetPayoutQuery(Guid Id, Guid Tenant) : IRequest<PayoutDto>;

public class GetPayoutQueryValidator : AbstractValidator<GetPayoutQuery>
{
    public GetPayoutQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Tenant).NotEmpty();
    }
}

public class GetPayoutQueryHandler(IPayoutRepository repository) : IRequestHandler<GetPayoutQuery, PayoutDto>
{
    public async Task<PayoutDto> Handle(GetPayoutQuery request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var payout = await repository.FindAsync<PayoutAggregate>(request.Id, cancellationToken);

        // A payout of another tenant is answered as missing: the id alone must not reveal it exists.
        ApplicationGuard.IsTrue(payout is null || payout.Tenant != request.Tenant, Errors.PayoutNotFound);

        return PayoutDto.From(payout!);
    }
}
