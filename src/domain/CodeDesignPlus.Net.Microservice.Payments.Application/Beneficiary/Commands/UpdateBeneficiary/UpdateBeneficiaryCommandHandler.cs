namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Commands.UpdateBeneficiary;

public class UpdateBeneficiaryCommandHandler(IBeneficiaryRepository repository, IUserContext user) : IRequestHandler<UpdateBeneficiaryCommand>
{
    public async Task Handle(UpdateBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<BeneficiaryAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.BeneficiaryNotFound);

        aggregate.Update(
            request.BankCode,
            request.BankName,
            request.AccountType,
            request.AccountNumber,
            request.DocumentType,
            request.DocumentNumber,
            request.HolderName,
            request.Country,
            request.Currency,
            request.SwiftBic,
            request.Iban,
            request.RoutingNumber
        );

        await repository.UpdateAsync(aggregate, cancellationToken);
    }
}
