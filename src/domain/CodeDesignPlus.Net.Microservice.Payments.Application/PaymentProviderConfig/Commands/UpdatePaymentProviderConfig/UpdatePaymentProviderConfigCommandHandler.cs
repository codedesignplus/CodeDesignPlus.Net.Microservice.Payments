namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Commands.UpdatePaymentProviderConfig;

public class UpdatePaymentProviderConfigCommandHandler(IPaymentProviderConfigRepository repository, IUserContext user) : IRequestHandler<UpdatePaymentProviderConfigCommand>
{
    public async Task Handle(UpdatePaymentProviderConfigCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<PaymentProviderConfigAggregate>(request.Id, user.Tenant, cancellationToken);
        ApplicationGuard.IsNull(aggregate, Errors.PaymentProviderConfigNotFound);

        aggregate!.Update(
            request.AccountId,
            request.MerchantId,
            request.ApiKey,
            request.ApiLogin,
            request.SecretKey,
            request.Currency,
            request.PaymentCountry,
            request.IsTest,
            request.IsActive,
            user.IdUser);

        await repository.UpdateAsync(aggregate, cancellationToken);
    }
}
