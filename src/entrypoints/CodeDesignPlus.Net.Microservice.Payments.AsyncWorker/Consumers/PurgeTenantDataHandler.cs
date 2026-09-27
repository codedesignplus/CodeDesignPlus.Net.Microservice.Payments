using CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.DomainEvents;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Consumers;

/// <summary>
/// Al purgarse una copropiedad, borra sus pagos, su configuración de pasarela y sus dispersiones.
/// </summary>
/// <remarks>
/// Lo publica ms-tenants cuando vence el plazo para restaurar una copropiedad eliminada, y puede llegar más de una
/// vez: borrar lo que ya no está es inofensivo.
/// <para>
/// <b>Se conservan los pagos de la venta de licencias</b> (<see cref="PaymentAggregate.LicensesModule"/>): son
/// registro de la plataforma, no de la copropiedad, y pueden llevar su <c>Tenant</c> si se compró desde dentro de
/// ella. Por eso los pagos se borran con <c>DeleteByTenantKeepingLicensesAsync</c> y no con el borrado genérico.
/// </para>
/// <para>
/// <c>PurgeTenantDataHandlerTest</c> recorre el dominio y exige que se purgue todo tipo con <c>Tenant</c>: un
/// agregado nuevo que no se añada aquí hace fallar la prueba (regla 47).
/// </para>
/// </remarks>
[QueueName<PaymentAggregate>("PurgeTenantDataHandler")]
public class PurgeTenantDataHandler(IPaymentRepository repository, ILogger<PurgeTenantDataHandler> logger) : IEventHandler<TenantPurgedDomainEvent>
{
    public async Task HandleAsync(TenantPurgedDomainEvent data, CancellationToken token)
    {
        var tenant = data.AggregateId;

        var payments = await repository.DeleteByTenantKeepingLicensesAsync(tenant, token);
        var configs = await repository.DeleteByTenantAsync<PaymentProviderConfigAggregate>(tenant, token);
        var beneficiaries = await repository.DeleteByTenantAsync<BeneficiaryAggregate>(tenant, token);
        var disbursements = await repository.DeleteByTenantAsync<DisbursementAggregate>(tenant, token);
        var rules = await repository.DeleteByTenantAsync<DisbursementRuleAggregate>(tenant, token);

        logger.LogInformation(
            "Tenant {TenantId} purged: {Payments} payments, {Configs} provider configs, {Beneficiaries} beneficiaries, {Disbursements} disbursements and {Rules} disbursement rules deleted",
            tenant, payments, configs, beneficiaries, disbursements, rules);
    }
}
