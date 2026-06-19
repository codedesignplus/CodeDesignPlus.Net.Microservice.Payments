namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Commands.UpdatePaymentProviderConfig;

[DtoGenerator]
public record UpdatePaymentProviderConfigCommand(
    Guid Id,
    int AccountId,
    string MerchantId,
    string ApiKey,
    string ApiLogin,
    string SecretKey,
    string Currency,
    string PaymentCountry,
    bool IsTest,
    bool IsActive
) : IRequest;

public class Validator : AbstractValidator<UpdatePaymentProviderConfigCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AccountId).GreaterThan(0);
        RuleFor(x => x.MerchantId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ApiLogin).NotEmpty().MaximumLength(256);
        RuleFor(x => x.SecretKey).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches(@"^[A-Z]{3}$");
        RuleFor(x => x.PaymentCountry).NotEmpty().Length(2).Matches(@"^[A-Z]{2}$");
    }
}
