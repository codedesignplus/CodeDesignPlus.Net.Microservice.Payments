namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Commands.DeactivateBeneficiary;

public class DeactivateBeneficiaryCommandHandler(IBeneficiaryRepository repository, IUserContext user) : IRequestHandler<DeactivateBeneficiaryCommand>
{
    public async Task Handle(DeactivateBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<BeneficiaryAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.BeneficiaryNotFound);

        aggregate.Deactivate();

        await repository.UpdateAsync(aggregate, cancellationToken);
    }
}
