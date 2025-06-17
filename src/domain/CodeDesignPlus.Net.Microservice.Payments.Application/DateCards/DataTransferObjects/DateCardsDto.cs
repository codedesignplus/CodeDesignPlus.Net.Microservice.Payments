namespace CodeDesignPlus.Net.Microservice.Payments.Application.DateCards.DataTransferObjects;

public class DateCardsDto : IDtoBase
{
    public required Guid Id { get; set; }
    public required string Month { get; set; }
    public required string Year { get; set; }
}