namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class SavedCardAggregate(Guid id) : AggregateRootBase(id)
{
    /// <summary>
    /// The owner of the saved card.
    /// </summary>
    public Guid UserId { get; private set; }
    /// <summary>
    /// PayU tokenization token (safe to store, not PCI-sensitive).
    /// </summary>
    public string Token { get; private set; } = null!;
    /// <summary>
    /// Masked card number, e.g., "403799******1984".
    /// </summary>
    public string MaskedNumber { get; private set; } = null!;
    /// <summary>
    /// Card franchise: VISA, MASTERCARD, AMEX, etc.
    /// </summary>
    public string Franchise { get; private set; } = null!;
    /// <summary>
    /// Name of the card holder.
    /// </summary>
    public string CardHolderName { get; private set; } = null!;
    /// <summary>
    /// Expiration date in format "2027/03".
    /// </summary>
    public string ExpirationDate { get; private set; } = null!;
    /// <summary>
    /// Last 4 digits of the card number.
    /// </summary>
    public string Last4Digits { get; private set; } = null!;
    /// <summary>
    /// Whether this is the user's default card.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Creates a new SavedCardAggregate instance.
    /// </summary>
    public static SavedCardAggregate Create(
        Guid id,
        Guid userId,
        string token,
        string maskedNumber,
        string franchise,
        string cardHolderName,
        string expirationDate,
        string last4Digits,
        Guid createdBy)
    {
        DomainGuard.GuidIsEmpty(id, Errors.SavedCardIdCannotBeEmpty);
        DomainGuard.GuidIsEmpty(userId, Errors.SavedCardUserIdCannotBeEmpty);
        DomainGuard.IsNullOrEmpty(token, Errors.SavedCardTokenCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(maskedNumber, Errors.SavedCardMaskedNumberCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(franchise, Errors.SavedCardFranchiseCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(cardHolderName, Errors.SavedCardCardHolderNameCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(expirationDate, Errors.SavedCardExpirationDateCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(last4Digits, Errors.SavedCardLast4DigitsCannotBeNullOrEmpty);

        var aggregate = new SavedCardAggregate(id)
        {
            UserId = userId,
            Token = token,
            MaskedNumber = maskedNumber,
            Franchise = franchise,
            CardHolderName = cardHolderName,
            ExpirationDate = expirationDate,
            Last4Digits = last4Digits,
            IsDefault = false,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        aggregate.AddEvent(SavedCardCreatedDomainEvent.Create(id, userId, maskedNumber, franchise, last4Digits));

        return aggregate;
    }

    /// <summary>
    /// Soft-deletes the saved card by deactivating it.
    /// </summary>
    public void Delete()
    {
        IsActive = false;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();

        AddEvent(SavedCardDeletedDomainEvent.Create(Id, UserId));
    }

    /// <summary>
    /// Sets this card as the default card.
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }

    /// <summary>
    /// Removes the default flag from this card.
    /// </summary>
    public void UnsetDefault()
    {
        IsDefault = false;
        UpdatedAt = SystemClock.Instance.GetCurrentInstant();
    }
}
