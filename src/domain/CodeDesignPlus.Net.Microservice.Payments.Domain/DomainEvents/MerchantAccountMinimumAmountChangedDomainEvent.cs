using CodeDesignPlus.Net.ValueObjects.Financial;
namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

/// <summary>
/// The tenant set, or cleared, the smallest payment it accepts online (pendings/660).
/// </summary>
[EventKey<MerchantAccountAggregate>(1, "MerchantAccountMinimumAmountChangedDomainEvent")]
public class MerchantAccountMinimumAmountChangedDomainEvent(
    Guid aggregateId,
    Guid tenant,
    Money? minimumAmount,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    /// <summary>
    /// The tenant that owns the account.
    /// </summary>
    public Guid Tenant { get; } = tenant;

    /// <summary>
    /// The minimum in minor units, or null when the tenant cleared it.
    /// </summary>
    public Money? MinimumAmount { get; } = minimumAmount;

    /// <summary>
    /// Creates the event.
    /// </summary>
    /// <param name="aggregateId">The account.</param>
    /// <param name="tenant">The tenant that owns it.</param>
    /// <param name="minimumAmount">The minimum, or null.</param>
    /// <returns>The event.</returns>
    public static MerchantAccountMinimumAmountChangedDomainEvent Create(Guid aggregateId, Guid tenant, Money? minimumAmount)
    {
        return new MerchantAccountMinimumAmountChangedDomainEvent(aggregateId, tenant, minimumAmount);
    }
}
