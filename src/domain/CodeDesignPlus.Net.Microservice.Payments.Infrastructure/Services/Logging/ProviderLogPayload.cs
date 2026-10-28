using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Logging;

/// <summary>
/// The version of a request to a payment provider that can be written to a log.
/// </summary>
/// <remarks>
/// Requests used to be logged whole, and with them card data and the merchant's keys. Logs end up in the observability
/// backend, where no secret may ever be (pendings/322). Members marked with <see cref="NotLoggedAttribute"/> are left
/// out.
/// </remarks>
public static class ProviderLogPayload
{
    private static readonly Newtonsoft.Json.JsonSerializerSettings Settings = new()
    {
        ContractResolver = new NotLoggedContractResolver(),
        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
    };

    /// <summary>
    /// Serializa la petición sin los campos que no se escriben en los logs.
    /// </summary>
    /// <typeparam name="T">El tipo de la petición.</typeparam>
    /// <param name="request">La petición que se envía a la pasarela.</param>
    /// <returns>El JSON de la petición sin los campos marcados.</returns>
    public static string Serialize<T>(T request) => JsonSerializer.Serialize(request, Settings);

    /// <summary>
    /// El mismo contrato que el de la petición real, pero sin los campos marcados.
    /// </summary>
    private sealed class NotLoggedContractResolver : DefaultContractResolver
    {
        public NotLoggedContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy { OverrideSpecifiedNames = false };
        }

        protected override JsonProperty CreateProperty(MemberInfo member, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (member.GetCustomAttribute<NotLoggedAttribute>() is not null)
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }
    }
}
