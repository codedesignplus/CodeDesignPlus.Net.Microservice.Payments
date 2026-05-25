namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.DomainEvents;

/// <summary>
/// Evento de integracion emitido por ms-invoicing cuando un documento financiero
/// ha sido pagado y requiere un desembolso (split) al beneficiario/propietario.
/// </summary>
[EventKey("FinancialDocumentAggregate", 1, "DisbursementRequiredDomainEvent", "ms-invoicing", "kappali")]
public class DisbursementRequiredDomainEvent(
    Guid aggregateId,
    Guid beneficiaryUserId,
    long amount,
    string currency,
    Guid tenant,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    /// <summary>El identificador del usuario propietario que debe recibir el desembolso.</summary>
    public Guid BeneficiaryUserId { get; } = beneficiaryUserId;

    /// <summary>El monto total pagado en minor units que debe ser desembolsado (antes de comisiones).</summary>
    public long Amount { get; } = amount;

    /// <summary>El codigo de moneda ISO 4217 del monto (ej. "COP", "USD").</summary>
    public string Currency { get; } = currency;

    /// <summary>El identificador del tenant (organizacion) al que pertenece el documento.</summary>
    public Guid Tenant { get; } = tenant;
}
