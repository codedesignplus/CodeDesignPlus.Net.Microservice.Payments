using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands.ConnectMerchantAccount;

/// <summary>
/// Connects the collection account of the tenant in session with the keys of its own merchant.
/// </summary>
/// <remarks>
/// The keys are validated against the provider before anything is stored, and their secrets go only to the Vault.
/// </remarks>
/// <param name="PublicKey">The merchant's public key.</param>
/// <param name="PrivateKey">The merchant's private key.</param>
/// <param name="EventsSecret">The secret the provider signs its events with.</param>
/// <param name="IntegritySecret">The secret that signs the hosted page.</param>
/// <param name="PayoutsPrincipalId">User principal of the payouts API; empty when payouts are not connected.</param>
/// <param name="PayoutsApiKey">Key of the payouts API; empty when payouts are not connected.</param>
/// <param name="PayoutsSourceAccountId">Provider account the payouts are paid from; empty when payouts are not connected.</param>
public record ConnectMerchantAccountCommand(string PublicKey, string PrivateKey, string EventsSecret, string IntegritySecret, string? PayoutsPrincipalId, string? PayoutsApiKey, string? PayoutsSourceAccountId) : IRequest<MerchantAccountDto>
{
    /// <summary>
    /// The keys, as the connector reads them.
    /// </summary>
    /// <returns>The keys of the command.</returns>
    public MerchantKeys ToKeys() => new(PublicKey, PrivateKey, EventsSecret, IntegritySecret, PayoutsPrincipalId, PayoutsApiKey, PayoutsSourceAccountId);

    /// <summary>
    /// Never prints a secret.
    /// </summary>
    /// <returns>A redacted description.</returns>
    public override string ToString() => $"{nameof(ConnectMerchantAccountCommand)} {{ PublicKey = {PublicKey} }}";
}

public class ConnectMerchantAccountCommandValidator : AbstractValidator<ConnectMerchantAccountCommand>
{
    public ConnectMerchantAccountCommandValidator()
    {
        RuleFor(x => x.PublicKey).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PrivateKey).NotEmpty().MaximumLength(256);
        RuleFor(x => x.EventsSecret).NotEmpty().MaximumLength(256);
        RuleFor(x => x.IntegritySecret).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PayoutsPrincipalId).MaximumLength(256);
        RuleFor(x => x.PayoutsApiKey).MaximumLength(256);
        RuleFor(x => x.PayoutsSourceAccountId).MaximumLength(256);
    }
}
