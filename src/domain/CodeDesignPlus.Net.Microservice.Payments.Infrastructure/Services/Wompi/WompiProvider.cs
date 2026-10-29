using System.Globalization;
using CodeDesignPlus.Net.Exceptions.Guards;
using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.gRpc.Clients.Services.Currencies;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi.Models;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Microsoft.AspNetCore.WebUtilities;
using AppErrors = CodeDesignPlus.Net.Microservice.Payments.Application.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;

/// <summary>
/// Wompi (Colombia): collects through its hosted page (Web Checkout), validates the keys a tenant connects and pays
/// third parties through «Pagos a terceros».
/// </summary>
/// <remarks>
/// Wompi counts money in cents of the currency, whatever the currency's own minor unit is, while this service keeps
/// money in the minor unit that ms-locations declares for the currency. Every amount crosses that boundary here and
/// nowhere else.
/// </remarks>
public sealed class WompiProvider(IWompiClient client, ICurrencyGrpc currencyGrpc, IOptions<WompiOptions> options, ILogger<WompiProvider> logger) : ICollectProvider, IMerchantCredentialsProvider, IPayoutProvider
{
    /// <summary>
    /// The code this provider is routed and stored with.
    /// </summary>
    public const string ProviderCode = "Wompi";

    /// <summary>
    /// Wompi's amounts are always in cents: two decimals, by its API contract.
    /// </summary>
    private const int CentsDigits = 2;

    /// <inheritdoc/>
    public string Code => ProviderCode;

    /// <inheritdoc/>
    public async Task<CheckoutSession> CreateCheckoutAsync(PaymentAggregate payment, MerchantCredentials credentials, Uri returnUrl, Instant expiresAt, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(credentials);

        var reference = payment.Id.ToString();
        var currency = payment.Total.Currency;
        var amountInCents = await ToCentsAsync(payment.Total, cancellationToken);

        // Milliseconds and a Z, the ISO 8601 form Wompi documents; the same text goes in the URL and in the signature.
        var expirationTime = expiresAt.ToDateTimeUtc().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

        var parameters = new Dictionary<string, string?>
        {
            ["public-key"] = credentials.PublicKey,
            ["currency"] = currency,
            ["amount-in-cents"] = amountInCents.ToString(CultureInfo.InvariantCulture),
            ["reference"] = reference,
            ["signature:integrity"] = WompiKeys.IntegritySignature(reference, amountInCents, currency, expirationTime, credentials.Secrets.IntegritySecret),
            ["redirect-url"] = returnUrl.ToString(),
            ["expiration-time"] = expirationTime
        };

        AddCustomerData(parameters, payment.Buyer);

        var url = QueryHelpers.AddQueryString(options.Value.CheckoutUrl.ToString(), parameters);

        return new CheckoutSession(new Uri(url));
    }

    /// <inheritdoc/>
    public async Task<ProviderNotification?> ReadNotificationAsync(string body, CancellationToken cancellationToken)
    {
        var root = WompiKeys.Parse(body);

        if (root is null || root.Value<string>("event") != "transaction.updated")
        {
            return null;
        }

        var transaction = root.SelectToken("data.transaction")?.ToObject<WompiTransaction>();

        if (transaction is null || string.IsNullOrWhiteSpace(transaction.Reference))
        {
            return null;
        }

        var outcome = await ToOutcomeAsync(transaction, root.Value<string>("environment"), cancellationToken);

        return new ProviderNotification(transaction.Reference, outcome);
    }

    /// <inheritdoc/>
    public bool IsAuthentic(string body, MerchantCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        return WompiKeys.IsAuthenticEvent(body, credentials.Secrets.EventsSecret);
    }

    /// <inheritdoc/>
    public async Task<CollectOutcome?> GetOutcomeAsync(PaymentAggregate payment, MerchantCredentials credentials, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(credentials);

        var transactions = await client.GetTransactionsByReferenceAsync(credentials.Secrets.PrivateKey, payment.Id.ToString(), cancellationToken);

        // A payer who is declined can try again on the same page, and each attempt is a transaction with the same
        // reference: an approved one wins, otherwise the latest one tells how it stands.
        var transaction = transactions.FirstOrDefault(x => x.Status == "APPROVED")
            ?? transactions.OrderByDescending(x => x.CreatedAt, StringComparer.Ordinal).FirstOrDefault();

        return transaction is null ? null : await ToOutcomeAsync(transaction, null, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MerchantValidation> ValidateAsync(string publicKey, MerchantSecrets secrets, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(secrets);

        var environment = WompiKeys.EnvironmentOf(publicKey);

        if (environment is null || !publicKey.StartsWith("pub_", StringComparison.Ordinal))
        {
            return new MerchantValidation(false, null, null, AppErrors.MerchantPublicKeyIsNotFromProvider);
        }

        if (WompiKeys.EnvironmentOf(secrets.PrivateKey) != environment || !secrets.PrivateKey.StartsWith("prv_", StringComparison.Ordinal))
        {
            return new MerchantValidation(false, environment, null, AppErrors.MerchantPrivateKeyDoesNotMatchPublicKey);
        }

        var merchant = await client.GetMerchantAsync(publicKey, cancellationToken);

        if (merchant is null)
        {
            return new MerchantValidation(false, environment, null, AppErrors.MerchantPublicKeyIsUnknown);
        }

        if (!merchant.Active)
        {
            return new MerchantValidation(false, environment, merchant.Name, AppErrors.MerchantIsInactive);
        }

        var accepted = await client.IsPrivateKeyAcceptedAsync(secrets.PrivateKey, cancellationToken);

        if (!accepted)
        {
            return new MerchantValidation(false, environment, merchant.Name, AppErrors.MerchantPrivateKeyWasRejected);
        }

        return new MerchantValidation(true, environment, merchant.LegalName ?? merchant.Name, null);
    }

    /// <inheritdoc/>
    public async Task<PayoutSubmission> SubmitAsync(PayoutOrder order, MerchantCredentials credentials, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(credentials);
        InfrastructureGuard.IsNullOrEmpty(credentials.PayoutsSourceAccountId, Errors.PaymentProviderKeyIsNotRecognized);

        var reference = order.PayoutId.ToString();

        var request = new WompiPayoutRequest
        {
            Reference = reference,
            AccountId = credentials.PayoutsSourceAccountId!,
            PaymentType = "PROVIDERS",
            Transactions =
            [
                new WompiPayoutTransaction
                {
                    LegalIdType = order.Beneficiary.LegalIdType,
                    LegalId = order.Beneficiary.LegalId,
                    BankId = order.Beneficiary.BankId,
                    AccountType = order.Beneficiary.AccountType,
                    AccountNumber = order.Beneficiary.AccountNumber,
                    Name = order.Beneficiary.Name,
                    Email = order.Beneficiary.Email,
                    Amount = await ToCentsAsync(order.Amount, cancellationToken),
                    Reference = reference
                }
            ]
        };

        var batch = await client.CreatePayoutAsync(credentials, request, reference, cancellationToken);

        InfrastructureGuard.IsNullOrEmpty(batch.Id, Errors.PaymentGatewayRejectedTheRequest);

        // A new batch waits for an approver of the merchant on Wompi's panel; anything else is already moving.
        var status = batch.Status == "PENDING_APPROVAL" ? PayoutStatus.AwaitingApproval : PayoutStatus.Processing;

        return new PayoutSubmission(batch.Id!, null, status);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PayoutBank>> GetBanksAsync(MerchantCredentials credentials, CancellationToken cancellationToken)
    {
        var banks = await client.GetBanksAsync(credentials, cancellationToken);

        return [.. banks.Where(x => !string.IsNullOrWhiteSpace(x.Id)).Select(x => new PayoutBank(x.Id!, x.Name ?? x.Id!))];
    }

    private void AddCustomerData(Dictionary<string, string?> parameters, Net.ValueObjects.User.Buyer buyer)
    {
        if (buyer is null)
        {
            return;
        }

        parameters["customer-data:email"] = buyer.Email;
        parameters["customer-data:full-name"] = buyer.Name;

        // The page takes the number and its country prefix apart; a number already written with its prefix cannot be
        // split without knowing the country, so it is left for the payer to type.
        if (!string.IsNullOrWhiteSpace(buyer.Phone) && !buyer.Phone.TrimStart().StartsWith('+'))
        {
            parameters["customer-data:phone-number"] = new string([.. buyer.Phone.Where(char.IsDigit)]);
        }

        var legalIdType = buyer.TypeDocument?.Code;

        if (!string.IsNullOrWhiteSpace(buyer.Document) && legalIdType is not null && options.Value.LegalIdTypes.Contains(legalIdType, StringComparer.OrdinalIgnoreCase))
        {
            parameters["customer-data:legal-id"] = buyer.Document;
            parameters["customer-data:legal-id-type"] = legalIdType.ToUpperInvariant();
        }
    }

    private async Task<CollectOutcome> ToOutcomeAsync(WompiTransaction transaction, string? environment, CancellationToken cancellationToken)
    {
        var status = transaction.Status switch
        {
            "APPROVED" => PaymentStatus.Succeeded,
            "DECLINED" or "VOIDED" or "ERROR" => PaymentStatus.Failed,
            _ => PaymentStatus.Pending
        };

        Money? amount = null;

        if (transaction.AmountInCents is { } cents && !string.IsNullOrWhiteSpace(transaction.Currency))
        {
            amount = await FromCentsAsync(cents, transaction.Currency, cancellationToken);
        }

        var raw = new Dictionary<string, string?>
        {
            ["provider"] = ProviderCode,
            ["transactionId"] = transaction.Id,
            ["status"] = transaction.Status,
            ["statusMessage"] = transaction.StatusMessage,
            ["paymentMethodType"] = transaction.PaymentMethodType,
            ["amountInCents"] = transaction.AmountInCents?.ToString(CultureInfo.InvariantCulture),
            ["currency"] = transaction.Currency,
            ["finalizedAt"] = transaction.FinalizedAt
        };

        if (environment is not null)
        {
            raw["environment"] = environment;
        }

        return new CollectOutcome(status, transaction.Id, amount, transaction.PaymentMethodType, raw);
    }

    private async Task<long> ToCentsAsync(Money money, CancellationToken cancellationToken)
    {
        var digits = await DecimalDigitsAsync(money.Currency, cancellationToken);

        if (digits <= CentsDigits)
        {
            return checked(money.Amount * Pow10(CentsDigits - digits));
        }

        var divisor = Pow10(digits - CentsDigits);

        InfrastructureGuard.IsTrue(money.Amount % divisor != 0, Errors.AmountCannotBeExpressedInCents);

        return money.Amount / divisor;
    }

    private async Task<Money?> FromCentsAsync(long cents, string currency, CancellationToken cancellationToken)
    {
        var digits = await DecimalDigitsAsync(currency, cancellationToken);

        if (digits >= CentsDigits)
        {
            return Money.FromLong(checked(cents * Pow10(digits - CentsDigits)), currency);
        }

        var divisor = Pow10(CentsDigits - digits);

        if (cents % divisor != 0)
        {
            logger.LogWarning("Wompi reported {Cents} cents of {Currency}, which is not a whole minor unit; the captured amount is left out.", cents, currency);

            return null;
        }

        return Money.FromLong(cents / divisor, currency);
    }

    private async Task<int> DecimalDigitsAsync(string currency, CancellationToken cancellationToken)
    {
        var response = await currencyGrpc.GetCurrencyAsync(new GetCurrencyRequest { Code = currency }, cancellationToken);

        return response.DecimalDigits;
    }

    private static long Pow10(int exponent)
    {
        var result = 1L;

        for (var i = 0; i < exponent; i++)
        {
            result *= 10;
        }

        return result;
    }
}
