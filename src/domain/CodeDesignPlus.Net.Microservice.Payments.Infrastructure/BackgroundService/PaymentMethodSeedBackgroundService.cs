using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Microservice.Payments.Application.PaymentMethod.Commands.CreatePaymentMethod;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using MediatR;
using H = Microsoft.Extensions.Hosting;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.BackgroundService;

public class PaymentMethodSeedBackgroundService(ILogger<PaymentMethodSeedBackgroundService> logger, IMediator mediator) : H.BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        return Task.Run(async () =>
        {
            try
            {
                foreach (var paymentMethod in paymentMethods)
                {
                    var command = new CreatePaymentMethodCommand(
                        paymentMethod.Id,
                        Provider.Payu,
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
        }, stoppingToken);
    }
    
    private readonly List<PaymentMethodAggregate> paymentMethods = new()
    {
        PaymentMethodAggregate.Create(
            Guid.Parse("b1f8c3d2-4e5f-4a6b-8c9d-0e1f2a3b4c5d"),
            Provider.Payu,
            "American Express",
            "AMEX",
            TypePaymentMethod.CreditCard,
            "Tarjeta de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("c2d3e4f5-6a7b-8c9d-0e1f2a3b4c5e"),
            Provider.Payu,
            "Banco de Bogotá",
            "BANK_REFERENCED",
            TypePaymentMethod.BankReference,
            "Referencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("d3e4f5a6-7b8c-9d0e-1f2a3b4c5d6e"),
            Provider.Payu,
            "Bancolombia",
            "BANK_REFERENCED",
            TypePaymentMethod.BankReference,
            "Referencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("e4f5a6b7-8c9d-0e1f2a3b4c5d6e7"),
            Provider.Payu,
            "Botón Bancolombia",
            "BANCOLOMBIA_BUTTON",
            TypePaymentMethod.BankTransfer,
            "Transferencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("f5a6b7c8-9d0e-1f2a3b4c5d6e7f8"),
            Provider.Payu,
            "Codensa",
            "CODENSA",
            TypePaymentMethod.CreditCard,
            "Tarjeta de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("a6b7c8d9-0e1f2a3b4c5d6e7f8g9"),
            Provider.Payu,
            "Davivienda",
            "BANK_REFERENCED",
            TypePaymentMethod.BankReference,
            "Referencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("b7c8d9e0-1f2a3b4c5d6e7f8g9h0"),
            Provider.Payu,
            "Diners",
            "DINERS",
            TypePaymentMethod.CreditCard,
            "Tarjeta de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("c8d9e0f1-2a3b4c5d6e7f8g9h0i1"),
            Provider.Payu,
            "Efecty",
            "EFECTY",
            TypePaymentMethod.Cash,
            "Efectivo"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("d9e0f1a2-3b4c5d6e7f8g9h0i1j2"),
            Provider.Payu,
            "Google Pay",
            "GOOGLE_PAY",
            TypePaymentMethod.MobilePaymentService,
            "Servicio móvil de pagos"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("e0f1a2b3-4c5d6e7f8g9h0i1j2k3"),
            Provider.Payu,
            "Mastercard",
            "MASTERCARD",
            TypePaymentMethod.CreditCard,
            "Tarjeta de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("f1a2b3c4-5d6e7f8g9h0i1j2k3l4"),
            Provider.Payu,
            "Mastercard",
            "MASTERCARD",
            TypePaymentMethod.DebitCard,
            "Tarjeta de débito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("a2b3c4d5-6e7f8g9h0i1j2k3l4m5"),
            Provider.Payu,
            "Nequi",
            "NEQUI",
            TypePaymentMethod.MobilePaymentService,
            "Servicio móvil de pagos"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("b3c4d5e6-7f8g9h0i1j2k3l4m5n6"),
            Provider.Payu,
            "PSE",
            "PSE",
            TypePaymentMethod.BankTransfer,
            "Transferencia bancaria"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("c4d5e6f7-8g9h0i1j2k3l4m5n6o7"),
            Provider.Payu,
            "Su Red",
            "OTHERS_CASH",
            TypePaymentMethod.Cash,
            "Oficinas de pago: PagaTodo, Gana Gana, Gana, Acertemos, Apuestas Cúcuta 75, Su Chance, La Perla, Apuestas Unidas, JER."
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("d5e6f7a8-9h0i1j2k3l4m5n6o7p8"),
            Provider.Payu,
            "VISA",
            "VISA",
            TypePaymentMethod.CreditCard,
            "Tarjetas de crédito"
        ),
        PaymentMethodAggregate.Create(
            Guid.Parse("e6f7a8b9-0i1j2k3l4m5n6o7p8q9"),
            Provider.Payu,
            "VISA",
            "VISA_DEBIT",
            TypePaymentMethod.DebitCard,
            "Tarjetas de débito"
        )
    };
}
