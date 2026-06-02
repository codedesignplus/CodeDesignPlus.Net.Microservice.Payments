namespace CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.DataTransferObjects;

public class SavedCardDto : IDtoBase
{
    public required Guid Id { get; set; }
    public string MaskedNumber { get; set; } = null!;
    public string Franchise { get; set; } = null!;
    public string CardHolderName { get; set; } = null!;
    public string ExpirationDate { get; set; } = null!;
    public string Last4Digits { get; set; } = null!;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}
