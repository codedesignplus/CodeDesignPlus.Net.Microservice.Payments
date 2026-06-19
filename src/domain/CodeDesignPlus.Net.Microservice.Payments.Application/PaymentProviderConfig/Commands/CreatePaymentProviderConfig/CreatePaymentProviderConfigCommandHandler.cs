namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Commands.CreatePaymentProviderConfig;

public class CreatePaymentProviderConfigCommandHandler(IPaymentProviderConfigRepository repository, IUserContext user) : IRequestHandler<CreatePaymentProviderConfigCommand>
{
    public async Task Handle(CreatePaymentProviderConfigCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exists = await repository.ExistsAsync<PaymentProviderConfigAggregate>(request.Id, user.Tenant, cancellationToken);
        ApplicationGuard.IsTrue(exists, Errors.PaymentProviderConfigAlreadyExists);

        var aggregate = PaymentProviderConfigAggregate.Create(
            request.Id,
            request.Provider,
            request.AccountId,
            request.MerchantId,
            request.ApiKey,
            request.ApiLogin,
            request.SecretKey,
            request.Currency,
            request.PaymentCountry,
            request.IsTest,
            user.Tenant,
            user.IdUser);

        await repository.CreateAsync(aggregate, cancellationToken);
    }
}
