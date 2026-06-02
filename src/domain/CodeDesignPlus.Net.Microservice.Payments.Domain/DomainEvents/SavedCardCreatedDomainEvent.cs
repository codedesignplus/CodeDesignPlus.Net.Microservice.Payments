namespace CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;

[EventKey<SavedCardAggregate>(1, "SavedCardCreatedDomainEvent")]
public class SavedCardCreatedDomainEvent(
    Guid aggregateId,
    Guid userId,
    string maskedNumber,
    string franchise,
    string last4Digits,
    Guid? eventId = null,
    Instant? occurredAt = null,
    Dictionary<string, object>? metadata = null
) : DomainEvent(aggregateId, eventId, occurredAt, metadata)
{
    public Guid UserId { get; } = userId;
    public string MaskedNumber { get; } = maskedNumber;
    public string Franchise { get; } = franchise;
    public string Last4Digits { get; } = last4Digits;

    public static SavedCardCreatedDomainEvent Create(
        Guid aggregateId,
        Guid userId,
        string maskedNumber,
        string franchise,
        string last4Digits,
        Guid? eventId = null,
        Instant? occurredAt = null,
        Dictionary<string, object>? metadata = null)
    {
        return new SavedCardCreatedDomainEvent(
            aggregateId,
            userId,
            maskedNumber,
            franchise,
            last4Digits,
            eventId ?? Guid.NewGuid(),
            occurredAt ?? SystemClock.Instance.GetCurrentInstant(),
            metadata
        );
    }
}
