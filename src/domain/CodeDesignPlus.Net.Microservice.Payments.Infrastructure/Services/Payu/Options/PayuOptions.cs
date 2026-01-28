using System.ComponentModel.DataAnnotations;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;

public class PayuOptions: IValidatableObject
{
    private readonly string[] LanguagesSupported = ["es", "en", "pt"];
    private readonly string[] CurrenciesSupported = ["COP", "USD", "MXN", "BRL", "PEN", "ARS", "CLP", "CRC", "GTQ", "HNL", "SVC"];

    public const string Section = "Payu";
    public bool Enable { get; set; }
    public int AccountId { get; set; }
    public string MerchantId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiLogin { get; set; } = string.Empty;
    public bool IsTest { get; set; } = true;
    public Uri Url { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public string TransactionType { get; set; } = "AUTHORIZATION_AND_CAPTURE";
    public string PaymentCountry { get; internal set; } = "CO";
    public string SecretKey { get; set; } = null!;
    public string NotificationUrl { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var result = new List<ValidationResult>();

        if (this.Enable)
        {
            if (this.AccountId <= 0)
                result.Add(new ValidationResult("AccountId must be greater than zero when Payu is enabled.", [nameof(AccountId)]));

            if (string.IsNullOrWhiteSpace(this.MerchantId))
                result.Add(new ValidationResult("MerchantId is required when Payu is enabled.", [nameof(MerchantId)]));

            if (string.IsNullOrWhiteSpace(this.ApiKey))
                result.Add(new ValidationResult("ApiKey is required when Payu is enabled.", [nameof(ApiKey)]));

            if (string.IsNullOrWhiteSpace(this.ApiLogin))
                result.Add(new ValidationResult("ApiLogin is required when Payu is enabled.", [nameof(ApiLogin)]));

            if (this.Url == null)
                result.Add(new ValidationResult("Url is required when Payu is enabled.", [nameof(Url)]));

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

            if (string.IsNullOrWhiteSpace(this.SecretKey))
                result.Add(new ValidationResult("SecretKey is required when Payu is enabled.", [nameof(SecretKey)]));
            
            if(this.SecretKey.Length < 36)
                result.Add(new ValidationResult("SecretKey must be at least 36 characters long when Payu is enabled.", [nameof(SecretKey)]));

            if (string.IsNullOrWhiteSpace(this.NotificationUrl))
                result.Add(new ValidationResult("NotificationUrl is required when Payu is enabled.", [nameof(NotificationUrl)]));
        }

        return result;
    }
}
