namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentMethod.Commands.CreatePaymentMethod;

public class CreatePaymentMethodCommandHandler(IPaymentMethodRepository repository) : IRequestHandler<CreatePaymentMethodCommand>
{
    public async Task Handle(CreatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exist = await repository.ExistsAsync<PaymentMethodAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.PaymentMethodAlreadyExists);

        var aggregate = PaymentMethodAggregate.Create(request.Id, request.Provider, request.Name, request.Code, request.Type, request.Comments);

        await repository.CreateAsync(aggregate, cancellationToken);
    }
}