namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Commands.DeletePaymentProviderConfig;

public class DeletePaymentProviderConfigCommandHandler(IPaymentProviderConfigRepository repository, IUserContext user) : IRequestHandler<DeletePaymentProviderConfigCommand>
{
    public async Task Handle(DeletePaymentProviderConfigCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<PaymentProviderConfigAggregate>(request.Id, user.Tenant, cancellationToken);
        ApplicationGuard.IsNull(aggregate, Errors.PaymentProviderConfigNotFound);

        aggregate!.Delete(user.IdUser);

        await repository.UpdateAsync(aggregate, cancellationToken);
    }
}
