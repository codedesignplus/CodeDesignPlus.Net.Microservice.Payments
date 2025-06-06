using System.ComponentModel.DataAnnotations;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;

public class PayuOptions: IValidatableObject
{
    private readonly string[] LanguagesSupported = ["es", "en", "pt"];
    private readonly string[] CurrenciesSupported = ["COP", "USD", "MXN", "BRL", "PEN", "ARS", "CLP", "CRC", "GTQ", "HNL", "SVC"];

    public const string Section = "Payu";
    public bool Enabled { get; set; }
    public int AccountId { get; set; }
    public string MerchantId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiLogin { get; set; } = string.Empty;
    public bool IsTest { get; set; } = true;
    public string BaseUrl { get; set; } = null!;
    public string Language { get; set; } = "es";
    public string Currency { get; set; } = "COP";
    public string TransactionType { get; set; } = "AUTHORIZATION_AND_CAPTURE";
    public string PaymentCountry { get; internal set; } = "CO";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var result = new List<ValidationResult>();

        if (this.Enabled)
        {
            if (this.AccountId <= 0)
                result.Add(new ValidationResult("AccountId must be greater than zero when Payu is enabled.", [nameof(AccountId)]));
                
            if (string.IsNullOrWhiteSpace(this.MerchantId))
                result.Add(new ValidationResult("MerchantId is required when Payu is enabled.", [nameof(MerchantId)]));

            if (string.IsNullOrWhiteSpace(this.ApiKey))
                result.Add(new ValidationResult("ApiKey is required when Payu is enabled.", [nameof(ApiKey)]));

            if (string.IsNullOrWhiteSpace(this.ApiLogin))
                result.Add(new ValidationResult("ApiLogin is required when Payu is enabled.", [nameof(ApiLogin)]));

            if (string.IsNullOrWhiteSpace(this.BaseUrl))
                result.Add(new ValidationResult("BaseUrl is required when Payu is enabled.", [nameof(BaseUrl)]));

            if (!Uri.IsWellFormedUriString(this.BaseUrl, UriKind.Absolute))
                result.Add(new ValidationResult("BaseUrl must be a valid absolute URI.", [nameof(BaseUrl)]));

            if (!LanguagesSupported.Contains(this.Language))
                result.Add(new ValidationResult($"Language must be one of the following: {string.Join(", ", LanguagesSupported)}.", [nameof(Language)]));

            if (!CurrenciesSupported.Contains(this.Currency))
                result.Add(new ValidationResult($"Currency must be one of the following: {string.Join(", ", CurrenciesSupported)}.", [nameof(Currency)]));

            if (string.IsNullOrWhiteSpace(this.TransactionType))
                result.Add(new ValidationResult("TransactionType is required when Payu is enabled.", [nameof(TransactionType)]));

            if (this.TransactionType != "AUTHORIZATION_AND_CAPTURE")
                result.Add(new ValidationResult("TransactionType must be either 'AUTHORIZATION_AND_CAPTURE' when Payu is enabled and country is 'CO'.", [nameof(TransactionType)]));

            if (string.IsNullOrWhiteSpace(this.PaymentCountry))
                result.Add(new ValidationResult("PaymentCountry is required when Payu is enabled.", [nameof(PaymentCountry)]));


        }

        return result;
    }
}
