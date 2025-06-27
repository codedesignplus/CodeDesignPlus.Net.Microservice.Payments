using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.Pay;

public class InitiatePaymentCommandHandler(IPaymentRepository repository, IUserContext user, IPubSub pubsub, ILogger<InitiatePaymentCommandHandler> logger, IPaymentProviderAdapterFactory adapterFactory) : IRequestHandler<InitiatePaymentCommand>
{
    public async Task Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
    {
        ApplicationGuard.IsNull(request, Errors.InvalidRequest);

        // 1. VALIDACIÓN
        // La validación de FluentValidation ya se ejecutó en el pipeline de MediatR.
        // Aquí validamos la lógica de negocio.

        bool exist;

        if (user.Tenant != Guid.Empty)
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, user.Tenant, cancellationToken);
        else
            exist = await repository.ExistsAsync<PaymentAggregate>(request.Id, cancellationToken);

        ApplicationGuard.IsTrue(exist, Errors.PaymentAlredyExists);

        // 2. SELECCIÓN DEL ADAPTADOR
        // Obtenemos el adaptador específico (PayU, MercadoPago) usando la fábrica.
        var adapter = adapterFactory.GetAdapter(request.PaymentProvider);

        // 4. CREACIÓN DEL AGREGADO EN ESTADO INICIAL
        // Creamos nuestro agregado agnóstico con estado 'Initiated'.
        // Ya no le pasamos objetos de solicitud/respuesta crudos.
        var payment = PaymentAggregate.Create(
            request.Id,
            request.Module,
            request.SubTotal,
            request.Tax,
            request.Total,
            request.Payer,
            request.PaymentMethod,
            request.Description,
            request.PaymentProvider,
            user.Tenant,
            user.IdUser
        );

        // 5. PERSISTENCIA INICIAL
        // Guardamos el pago ANTES de intentar contactar al proveedor.
        // Si el proveedor falla, ya tenemos un registro del intento.
        await repository.CreateAsync(payment, cancellationToken);

        // 6. LLAMADA AL PROVEEDOR EXTERNO (a través del Adaptador)
        // El handler no sabe cómo hablar con PayU, solo le pasa el comando al adaptador.
        var providerResult = await adapter.InitiatePaymentAsync(request, cancellationToken);

        // 7. ACTUALIZACIÓN POST-LLAMADA (Opcional, si hay datos inmediatos)
        // Si el proveedor nos da un ID de transacción de inmediato, lo guardamos.
        if (!string.IsNullOrEmpty(providerResult.ProviderTransactionId))
        {
            // Nota: necesitaríamos un método en el agregado para esto, ej: payment.SetProviderTransactionId(...)
        }

        // 8. PUBLICACIÓN DE EVENTOS DE DOMINIO
        // Publicamos el evento limpio 'PaymentInitiated'.
        await pubsub.PublishAsync(payment.GetAndClearEvents(), cancellationToken);
    }
}