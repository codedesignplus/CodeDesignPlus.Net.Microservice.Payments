namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentMethod.Commands.UpdatePaymentMethod;

public class UpdatePaymentMethodCommandHandler(IPaymentMethodRepository repository) : IRequestHandler<UpdatePaymentMethodCommand>
{
    public async Task Handle(UpdatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var aggregate = await repository.FindAsync<PaymentMethodAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsNull(aggregate, Errors.PaymentMethodNotFound);

        aggregate.Update(request.Provider, request.Name, request.Code, request.Type, request.Comments, request.IsActive);

        await repository.UpdateAsync(aggregate, cancellationToken);
    }
}