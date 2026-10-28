namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Logging;

/// <summary>
/// Marks a member of a request that travels to the payment provider but is never written to a log.
/// </summary>
/// <remarks>
/// It goes on the model, next to the member, so whoever adds another sensitive value marks it where it is declared and
/// not in a separate list of names that gets forgotten (pendings/322).
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotLoggedAttribute : Attribute;
