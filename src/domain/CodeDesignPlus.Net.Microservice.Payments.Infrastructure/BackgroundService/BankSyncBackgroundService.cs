using CodeDesignPlus.Net.Microservice.Payments.Application.Banks.Commands.SyncBanks;
using CodeDesignPlus.Net.Microservice.Payments.Application.Banks.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;
using MediatR;
using H = Microsoft.Extensions.Hosting;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.BackgroundService;

public class BankSyncBackgroundService(ILogger<BankSyncBackgroundService> logger, IServiceScopeFactory serviceScopeFactory) : H.BackgroundService
{

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceScopeFactory.CreateScope();

                var payu = scope.ServiceProvider.GetRequiredService<IPayu>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                try
                {
                    var banks = await payu.GetBanksListAsync(stoppingToken);

                    if (banks == null)
                    {
                        logger.LogWarning("No banks data received from PayU.");
                        continue;
                    }

                    if (banks.Code != "SUCCESS")
                    {
                        logger.LogWarning("Failed to retrieve banks: {Error}", banks.Error);
                        continue;
                    }

                    var command = new SyncBanksCommand([.. banks.Banks.Select(x => new BanksDto
                    {
                        Id = Guid.Parse(x.Id),
                        Description = x.Description,
                        Code = x.PseCode,
                        IsActive = true
                    })]);

                    await mediator.Send(command, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while syncing banks.");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        },  stoppingToken);
    }
}
