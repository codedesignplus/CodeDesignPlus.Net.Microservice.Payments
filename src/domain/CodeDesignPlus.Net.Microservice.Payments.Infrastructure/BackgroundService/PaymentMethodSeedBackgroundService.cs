using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentMethod.Commands.CreatePaymentMethod;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using MediatR;
using H = Microsoft.Extensions.Hosting;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.BackgroundService;

public class PaymentMethodSeedBackgroundService(ILogger<PaymentMethodSeedBackgroundService> logger, IServiceScopeFactory serviceScopeFactory) : H.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            foreach (var paymentMethod in paymentMethods)
            {
                var command = new CreatePaymentMethodCommand(
                    paymentMethod.Id,
                    PaymentProvider.Payu,
                    paymentMethod.Name,
                    paymentMethod.Code,
                    paymentMethod.Type,
                    paymentMethod.Comments
                );

                await mediator.Send(command, stoppingToken);
            }
        }
        catch (CodeDesignPlusException ex)
        {
            logger.LogWarning(ex, "An error occurred while seeding payment methods: {Message}", ex.Message);
        }
    }

    private readonly List<PaymentMethodAggregate> paymentMethods = new()
    {
        PaymentMethodAggregate.Create(
            Guid.Parse("1f2d55b8-45af-4ac7-ac8f-c07f8b7b560e"),
            PaymentProvider.Payu,
            "American Express",
            "AMEX",
            TypePaymentMethod.CreditCard,
            "Tarjeta de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("08da70c3-4cf4-4049-ab16-581d894dc3f8"),
            PaymentProvider.Payu,
            "Banco de Bogotá",
            "BANK_REFERENCED",
            TypePaymentMethod.BankReference,
            "Referencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("965f7119-2cd2-442e-8d00-27f4ecf21001"),
            PaymentProvider.Payu,
            "Bancolombia",
            "BANK_REFERENCED",
            TypePaymentMethod.BankReference,
            "Referencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("428a7532-3474-41e0-8df1-c71bf86e8277"),
            PaymentProvider.Payu,
            "Botón Bancolombia",
            "BANCOLOMBIA_BUTTON",
            TypePaymentMethod.BankTransfer,
            "Transferencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("a9ab45a3-9c10-441a-8864-4914eae77e0f"),
            PaymentProvider.Payu,
            "Codensa",
            "CODENSA",
            TypePaymentMethod.CreditCard,
            "Tarjeta de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("25a0231e-eead-4da7-9e20-d2769a9a7ca8"),
            PaymentProvider.Payu,
            "Davivienda",
            "BANK_REFERENCED",
            TypePaymentMethod.BankReference,
            "Referencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("fe34f156-3764-4d15-be83-f1d3a8b96757"),
            PaymentProvider.Payu,
            "Diners",
            "DINERS",
            TypePaymentMethod.CreditCard,
            "Tarjeta de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("0534f45f-f8ac-4417-a6ec-d7a04288834e"),
            PaymentProvider.Payu,
            "Efecty",
            "EFECTY",
            TypePaymentMethod.Cash,
            "Efectivo"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("b35b8de4-470f-4ce9-b2f6-08cb1943aa91"),
            PaymentProvider.Payu,
            "Google Pay",
            "GOOGLE_PAY",
            TypePaymentMethod.MobilePaymentService,
            "Servicio móvil de pagos"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("127a18e4-c7c9-4e89-9866-3720bb937f3c"),
            PaymentProvider.Payu,
            "Mastercard Crédito",
            "MASTERCARD",
            TypePaymentMethod.CreditCard,
            "Tarjeta de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("451f7883-e297-4e96-88ac-bb1d1901efd2"),
            PaymentProvider.Payu,
            "Mastercard Débito",
            "MASTERCARD",
            TypePaymentMethod.DebitCard,
            "Tarjeta de débito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("d98557cf-4caa-4fe1-820a-4baeb9dfb2bc"),
            PaymentProvider.Payu,
            "Nequi",
            "NEQUI",
            TypePaymentMethod.MobilePaymentService,
            "Servicio móvil de pagos"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("427eb569-f759-4fc3-97a6-51a0d93e7f10"),
            PaymentProvider.Payu,
            "PSE",
            "PSE",
            TypePaymentMethod.BankTransfer,
            "Transferencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("b4eee155-ff14-4f73-8ef5-9406c8495db7"),
            PaymentProvider.Payu,
            "Su Red",
            "OTHERS_CASH",
            TypePaymentMethod.Cash,
            "Oficinas de pago: PagaTodo, Gana Gana, Gana, Acertemos, Apuestas Cúcuta 75, Su Chance, La Perla, Apuestas Unidas, JER."
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("13b545e6-5f84-4306-a9c7-c94f6432f509"),
            PaymentProvider.Payu,
            "Visa Crédito",
            "VISA",
            TypePaymentMethod.CreditCard,
            "Tarjetas de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("87d5c766-bf3e-437f-a305-b333010adf51"),
            PaymentProvider.Payu,
            "Visa Débito",
            "VISA_DEBIT",
            TypePaymentMethod.DebitCard,
            "Tarjetas de débito"
        )
    };
}
