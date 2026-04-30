using System;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public partial record CreditCardToken
{
    [GeneratedRegex(@"^\d{4}/\d{2}$", RegexOptions.Compiled)]
    private static partial Regex ExpirationDateRegex();

    public string Last4Digits { get; private set; } = null!;
    public string CardHolderName { get; private set; } = null!;
    public string CreditCardTokenId { get; private set; } = null!;
    public string ExpirationDate { get; private set; } = null!;
    public int InstallmentsNumber { get; private set; }
    [BsonIgnore]
    public string SecurityCode { get; private set; } = null!;

    [JsonConstructor]
    private CreditCardToken(string last4Digits, string cardHolderName, string creditCardTokenId, string expirationDate, string securityCode, int? installmentsNumber = null)
    {
        var normalizedToken = creditCardTokenId?.Trim() ?? string.Empty;
        var normalizedExp = expirationDate?.Trim() ?? string.Empty;
        var normalizedSecurityCode = securityCode?.Trim() ?? string.Empty;

        DomainGuard.IsNullOrEmpty(normalizedToken, Errors.CreditCardTokenIdCannotBeNullOrEmpty);

        DomainGuard.IsNullOrEmpty(normalizedExp, Errors.CreditCardExpirationDateCannotBeNullOrEmpty);
        DomainGuard.IsFalse(ExpirationDateRegex().IsMatch(normalizedExp), Errors.CreditCardExpirationDateMustBeValidFormat);

        DomainGuard.IsNullOrEmpty(last4Digits, Errors.CreditCardLast4DigitsCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(cardHolderName, Errors.CreditCardCardHolderNameCannotBeNullOrEmpty);
        DomainGuard.IsNullOrEmpty(normalizedSecurityCode, Errors.CreditCardSecurityCodeCannotBeNullOrEmpty);

        Last4Digits = last4Digits.Trim();
        CardHolderName = cardHolderName.Trim();
        SecurityCode = normalizedSecurityCode;

        CreditCardTokenId = normalizedToken;
        ExpirationDate = normalizedExp;
        InstallmentsNumber = installmentsNumber ?? 1;
    }

    public static CreditCardToken Create(string last4Digits, string cardHolderName, string creditCardTokenId, string expirationDate, string securityCode, int? installmentsNumber = null)
    {
        return new CreditCardToken(last4Digits, cardHolderName, creditCardTokenId, expirationDate, securityCode, installmentsNumber);
    }
}
