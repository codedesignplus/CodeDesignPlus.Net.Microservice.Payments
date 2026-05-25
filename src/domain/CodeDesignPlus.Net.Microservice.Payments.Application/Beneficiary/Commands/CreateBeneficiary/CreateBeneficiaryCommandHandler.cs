namespace CodeDesignPlus.Net.Microservice.Payments.Application.Beneficiary.Commands.CreateBeneficiary;

public class CreateBeneficiaryCommandHandler(IBeneficiaryRepository repository, IUserContext user) : IRequestHandler<CreateBeneficiaryCommand>
{
    public async Task Handle(CreateBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exist = await repository.ExistsAsync<BeneficiaryAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.BeneficiaryAlreadyExists);

        var aggregate = BeneficiaryAggregate.Create(
            request.Id,
            request.UserId,
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
            request.RoutingNumber,
            user.Tenant
        );

        await repository.CreateAsync(aggregate, cancellationToken);
    }
}
