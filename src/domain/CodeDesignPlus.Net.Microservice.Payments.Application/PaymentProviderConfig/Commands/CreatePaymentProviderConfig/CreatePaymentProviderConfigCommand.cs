namespace CodeDesignPlus.Net.Microservice.Payments.Application.PaymentProviderConfig.Commands.CreatePaymentProviderConfig;

[DtoGenerator]
public record CreatePaymentProviderConfigCommand(
    Guid Id,
    PaymentProvider Provider,
    int AccountId,
    string MerchantId,
    string ApiKey,
    string ApiLogin,
    string SecretKey,
    string Currency,
    string PaymentCountry,
    bool IsTest
) : IRequest;

public class Validator : AbstractValidator<CreatePaymentProviderConfigCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Provider).IsInEnum();
        RuleFor(x => x.AccountId).GreaterThan(0);
        RuleFor(x => x.MerchantId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ApiLogin).NotEmpty().MaximumLength(256);
        RuleFor(x => x.SecretKey).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches(@"^[A-Z]{3}$");
        RuleFor(x => x.PaymentCountry).NotEmpty().Length(2).Matches(@"^[A-Z]{2}$");
    }
}
