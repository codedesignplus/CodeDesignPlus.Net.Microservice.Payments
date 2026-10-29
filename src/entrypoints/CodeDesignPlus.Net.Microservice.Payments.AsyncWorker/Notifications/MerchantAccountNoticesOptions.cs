using System.ComponentModel.DataAnnotations;

namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Notifications;

/// <summary>
/// Who is told when a collection account stops working, and how the notice is labelled. Configuration and not code:
/// this service does not know the roles of the product that uses it.
/// </summary>
public class MerchantAccountNoticesOptions : IValidatableObject
{
    /// <summary>
    /// Name of the section in the configuration.
    /// </summary>
    public static readonly string Section = "MerchantAccountNotices";

    /// <summary>
    /// Ids of the roles that receive the notice in the tenant (the catalog ids, never their names).
    /// </summary>
    public List<Guid> Roles { get; set; } = [];

    /// <summary>
    /// Stable key of the kind of notice, which the frontend knows how to render.
    /// </summary>
    [Required]
    public string Kind { get; set; } = null!;

    /// <summary>
    /// Module the click on the notice leads to.
    /// </summary>
    [Required]
    public string Module { get; set; } = null!;

    /// <summary>
    /// Language the notice is written in: the inbox stores the text, and a worker has no request to take it from.
    /// </summary>
    [Required]
    public string Language { get; set; } = "es";

    /// <summary>
    /// Author of the notice: the product's system user.
    /// </summary>
    public Guid SentBy { get; set; }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Roles.Count == 0 || Roles.Contains(Guid.Empty))
        {
            yield return new ValidationResult("MerchantAccountNotices:Roles needs at least one role id.", [nameof(Roles)]);
        }

        if (SentBy == Guid.Empty)
        {
            yield return new ValidationResult("MerchantAccountNotices:SentBy is required.", [nameof(SentBy)]);
        }
    }
}
