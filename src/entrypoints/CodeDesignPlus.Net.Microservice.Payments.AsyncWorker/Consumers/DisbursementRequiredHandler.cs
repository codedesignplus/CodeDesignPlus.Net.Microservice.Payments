using CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.DomainEvents;
using Microsoft.Extensions.Logging;

namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Consumers;

/// <summary>
/// Consumer que escucha <see cref="DisbursementRequiredDomainEvent"/> de ms-invoicing.
/// Calcula la comision de la plataforma y crea un registro de desembolso pendiente
/// para que sea procesado y transferido al propietario beneficiario.
/// </summary>
[QueueName<DisbursementAggregate>("DisbursementRequiredHandler")]
public class DisbursementRequiredHandler(
    IBeneficiaryRepository beneficiaryRepository,
    IDisbursementRuleRepository ruleRepository,
    IDisbursementRepository disbursementRepository,
    IPubSub pubsub,
    ILogger<DisbursementRequiredHandler> logger
) : IEventHandler<DisbursementRequiredDomainEvent>
{
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public async Task HandleAsync(DisbursementRequiredDomainEvent data, CancellationToken token)
    {
        // Idempotency check: use the document ID (AggregateId) as disbursement ID
        var exists = await disbursementRepository.ExistsAsync<DisbursementAggregate>(data.AggregateId, data.Tenant, token);

        if (exists)
        {
            logger.LogInformation("Disbursement already exists for document {DocumentId}. Skipping.", data.AggregateId);
            return;
        }

        // Find beneficiary by user ID
        var beneficiary = await beneficiaryRepository.GetByUserIdAsync(data.BeneficiaryUserId, data.Tenant, token);

        if (beneficiary == null)
        {
            logger.LogWarning("No beneficiary found for user {UserId} in tenant {Tenant}. Skipping disbursement for document {DocumentId}.",
                data.BeneficiaryUserId, data.Tenant, data.AggregateId);
            return;
        }

        // Get disbursement rule for tenant
        var rule = await ruleRepository.GetActiveByTenantAsync(data.Tenant, token);

        // Calculate commission
        long commission = 0;

        if (rule != null)
        {
            commission = rule.CommissionType == CommissionType.Fixed
                ? rule.CommissionAmount
                : (long)(data.Amount * rule.CommissionAmount / 10000m); // basis points (e.g., 200 = 2%)
        }

        var disbursedAmount = data.Amount - commission;

        // Create disbursement aggregate
        var disbursement = DisbursementAggregate.Create(
            data.AggregateId, // use document ID as disbursement ID (idempotent)
            data.AggregateId, // paymentId = document that was paid
            beneficiary.Id,
            data.BeneficiaryUserId,
            data.Amount,
            commission,
            disbursedAmount,
            data.Currency,
            data.Tenant
        );

        await disbursementRepository.CreateAsync(disbursement, token);
        await pubsub.PublishAsync(disbursement.GetAndClearEvents(), token);

        logger.LogInformation(
            "Disbursement created for document {DocumentId}. Amount: {Amount}, Commission: {Commission}, Disbursed: {Disbursed}, Beneficiary: {BeneficiaryId}",
            data.AggregateId, data.Amount, commission, disbursedAmount, beneficiary.Id);
    }
}
