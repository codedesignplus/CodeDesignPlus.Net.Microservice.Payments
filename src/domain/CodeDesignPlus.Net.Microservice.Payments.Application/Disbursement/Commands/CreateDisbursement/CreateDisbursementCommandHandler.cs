namespace CodeDesignPlus.Net.Microservice.Payments.Application.Disbursement.Commands.CreateDisbursement;

public class CreateDisbursementCommandHandler(IDisbursementRepository repository, IUserContext user, IPubSub pubsub) : IRequestHandler<CreateDisbursementCommand>
{
    public async Task Handle(CreateDisbursementCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var exist = await repository.ExistsAsync<DisbursementAggregate>(request.Id, user.Tenant, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.DisbursementAlreadyExists);

        var aggregate = DisbursementAggregate.Create(
            request.Id,
            request.PaymentId,
            request.BeneficiaryId,
            request.BeneficiaryUserId,
            request.TotalAmount,
            request.CommissionAmount,
            request.DisbursedAmount,
            request.Currency,
            user.Tenant
        );

        await repository.CreateAsync(aggregate, cancellationToken);

        await pubsub.PublishAsync(aggregate.GetAndClearEvents(), cancellationToken);
    }
}
