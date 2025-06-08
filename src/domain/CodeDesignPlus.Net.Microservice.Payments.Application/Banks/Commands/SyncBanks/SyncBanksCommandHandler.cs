namespace CodeDesignPlus.Net.Microservice.Payments.Application.Banks.Commands.SyncBanks;

public class SyncBanksCommandHandler(IBankRepository repository) : IRequestHandler<SyncBanksCommand>
{
    public async Task Handle(SyncBanksCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        var banks = await repository.GetAllAsync(cancellationToken);

        foreach (var bank in request.Banks)
        {
            var existingBank = banks.FirstOrDefault(x => x.Code == bank.Code);

            if (existingBank != null)
            {
                existingBank.Update( bank.Name, bank.Description, bank.IsActive);

                await repository.UpdateAsync(existingBank, cancellationToken);
            }
            else
            {
                var newBank = BanksAggregate.Create(bank.Id, bank.Name, bank.Description, bank.Code, bank.IsActive);

                await repository.CreateAsync(newBank, cancellationToken);
            }
        }
    }
}