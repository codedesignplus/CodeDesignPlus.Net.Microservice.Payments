using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Logging;

/// <summary>
/// Da la versión de una petición a PayU que se puede escribir en un log.
/// </summary>
/// <remarks>
/// La petición se registraba entera, y con ella el código de seguridad de la tarjeta, el número completo al tokenizarla y
/// la clave del comercio. Los logs acaban en SigNoz, y PCI DSS (requisito 3.2) no deja conservar el CVV en ninguna parte
/// después de la autorización (pendings/322). Los campos marcados con <see cref="NotLoggedAttribute"/> no salen.
/// </remarks>
public static class PayuLogPayload
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
