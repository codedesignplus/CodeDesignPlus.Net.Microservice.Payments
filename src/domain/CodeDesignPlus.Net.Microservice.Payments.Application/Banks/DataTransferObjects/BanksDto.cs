namespace CodeDesignPlus.Net.Microservice.Payments.Application.Banks.DataTransferObjects;

public class BanksDto : IDtoBase
{
    public required Guid Id { get; set; }
    public required string Description { get; set; }
    public required string Code { get; set; }
    public required bool IsActive { get; set; }
}