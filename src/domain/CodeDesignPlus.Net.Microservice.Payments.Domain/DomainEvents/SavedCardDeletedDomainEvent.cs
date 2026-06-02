namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<SavedCardAggregate>(2, "SavedCardDeletedDomainEvent")]
public class SavedCardDeletedDomainEvent(
    Guid aggregateId,
    Guid userId,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid UserId { get; } = userId;

    public static SavedCardDeletedDomainEvent Create(
        Guid aggregateId,
        Guid userId,
        Guid? eventId = null,
        Instant? occurredAt = null,
        Dictionary<string, object>? metadata = null)
    {
        return new SavedCardDeletedDomainEvent(
            aggregateId,
            userId,
            eventId ?? Guid.NewGuid(),
            occurredAt ?? SystemClock.Instance.GetCurrentInstant(),
            metadata
        );
    }
}
