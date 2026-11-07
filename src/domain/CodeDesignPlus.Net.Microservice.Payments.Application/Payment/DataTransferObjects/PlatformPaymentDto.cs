using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Payment.DataTransferObjects;

/// <summary>
/// A payment charged to the platform's own merchant account (a license of ExcellenceForge), as the Super Admin reads it:
/// where it went through, how it ended and what the provider kept (pendings/674).
/// </summary>
/// <remarks>
/// Like <see cref="PaymentSummaryDto"/>, it never carries the buyer's document nor the provider's raw answers; the mapping
/// lists each field (<c>MapsterConfigPayment</c>) and <c>PaymentSummaryDtoHasNoSecretsTest</c> walks this type too. The
/// commission was only in Mongo: the gross, the commission, its tax and the net come from what the provider reported when
/// the payment was approved.
/// </remarks>
public class PlatformPaymentDto : IDtoBase
{
    /// <summary>
    /// Id of the payment.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// The module that asked for it: <c>Licenses</c> for every license order.
    /// </summary>
    public string Module { get; set; } = null!;

    /// <summary>
    /// The record of the module it charges: the id of the order in ms-licenses.
    /// </summary>
    public Guid ReferenceId { get; set; }

    /// <summary>
    /// What it charges, as the module described it.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Name of the buyer; never their document.
    /// </summary>
    public string BuyerName { get; set; } = null!;

    /// <summary>
    /// Where the payment stands.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Why it did not go through; <see cref="PaymentFailureReason.None"/> while open or when it succeeded.
    /// </summary>
    public PaymentFailureReason FailureReason { get; set; }

    /// <summary>
    /// What the bank or the provider said when it did not go through, as they said it.
    /// </summary>
    public string? ProviderMessage { get; set; }

    /// <summary>
    /// Whether the provider approved it after the platform had closed it as failed or expired.
    /// </summary>
    public bool ApprovedLate { get; set; }

    /// <summary>
    /// Code of the provider that processed it (for example <c>Wompi</c>).
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// Payment method the payer chose on the provider's page (for example <c>CARD</c>, <c>NEQUI</c> or <c>PSE</c>); null
    /// until the provider reports it.
    /// </summary>
    public string? PaymentMethodType { get; set; }

    /// <summary>
    /// Id of the transaction at the provider; null until the provider reports it.
    /// </summary>
    public string? ProviderTransactionId { get; set; }

    /// <summary>
    /// When it was started.
    /// </summary>
    public Instant CreatedAt { get; set; }

    /// <summary>
    /// When it last changed: for a closed payment, when it closed.
    /// </summary>
    public Instant? UpdatedAt { get; set; }

    /// <summary>
    /// What was charged with its tax, in minor units: the gross asked of the buyer.
    /// </summary>
    public Money Total { get; set; } = null!;

    /// <summary>
    /// What the provider captured; null unless it succeeded.
    /// </summary>
    public Money? CapturedAmount { get; set; }

    /// <summary>
    /// The commission the provider keeps, without its tax; null while it is not known.
    /// </summary>
    public Money? ProviderFee { get; set; }

    /// <summary>
    /// The tax on <see cref="ProviderFee"/>; null while it is not known.
    /// </summary>
    public Money? ProviderFeeTax { get; set; }

    /// <summary>
    /// What reaches the account: <see cref="CapturedAmount"/> less the commission and its tax; null while the commission is
    /// not known.
    /// </summary>
    public Money? NetAmount { get; set; }

    /// <summary>
    /// Where the commission came from: reported by the provider or computed from the account's fee schedule
    /// (<see cref="Domain.ValueObjects.ProviderFeeSources"/>).
    /// </summary>
    public string? FeeSource { get; set; }
}
