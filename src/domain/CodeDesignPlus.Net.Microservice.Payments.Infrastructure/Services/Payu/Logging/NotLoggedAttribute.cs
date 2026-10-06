namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Logging;

/// <summary>
/// Marca un campo de una petición a la pasarela que viaja a la pasarela pero no se escribe en los logs.
/// </summary>
/// <remarks>
/// Se pone en el modelo, junto al campo, para que quien añada otro dato sensible lo marque donde lo declara y no en una
/// lista de nombres aparte que se olvida (pendings/322).
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotLoggedAttribute : Attribute;
