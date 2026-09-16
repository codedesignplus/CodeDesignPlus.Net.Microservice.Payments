using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeDesignPlus.Net.Microservice.Payments.Application.Setup;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
// El namespace del micro tiene un segmento "Payment", y la busqueda del namespace envolvente gana a
// las directivas using: "PaymentMethod" a secas resuelve a un namespace, no al objeto de valor. Un
// alias con otro nombre lo esquiva.
using MetodoDePago = CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod;
using Tarjeta = CodeDesignPlus.Net.ValueObjects.Payment.CreditCard;
using TransferenciaPse = CodeDesignPlus.Net.ValueObjects.Payment.Pse;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.DataTransferObjects;

/// <summary>
/// Vigila que el contrato de consulta de pagos no publique secretos.
/// </summary>
/// <remarks>
/// El agregado guarda el token de la pasarela, el titular de la tarjeta, su fecha de vencimiento, el codigo
/// de seguridad y el documento del comprador. Ninguno de esos puede salir por REST, y la unica razon por la
/// que hoy no salen es que alguien escribio el DTO con cuidado. Eso se olvida.
/// <para>
/// Por eso no se comprueba campo por campo con igualdades, sino recorriendo el tipo por reflexion contra una
/// lista de nombres prohibidos, y <b>entrando en los tipos anidados</b>: la forma realista de reintroducir la
/// fuga no es escribir <c>SecurityCode</c> en el DTO, es exponer el objeto <c>PaymentMethod</c> entero
/// porque resulta comodo.
/// </para>
/// </remarks>
public class PaymentSummaryDtoHasNoSecretsTest
{
    /// <summary>
    /// Los nombres que no pueden aparecer en ninguna parte del contrato.
    /// </summary>
    /// <remarks>
    /// Los cinco primeros son datos que identifican a una persona o permiten cobrarle. Los dos ultimos son
    /// la respuesta cruda del proveedor: no son secretos por si mismos, pero su contenido lo decide PayU y
    /// no nosotros, asi que publicarlos es firmar en blanco.
    /// <para>
    /// Se comparan por contencion y sin distinguir mayusculas, para que un <c>BuyerDocument</c> o un
    /// <c>CardToken</c> caigan igual que el nombre exacto.
    /// </para>
    /// </remarks>
    private static readonly string[] Prohibidos =
    [
        "Token",
        "SecurityCode",
        "CardHolderName",
        "Document",
        "ExpirationDate",
        "InitiateResponse",
        "FinalResponse"
    ];

    private static readonly Guid PaymentId = Guid.Parse("aa000000-0000-4000-8000-000000000001");
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    private static readonly Guid Comprador = Guid.Parse("cc000000-0000-4000-8000-000000000003");
    private static readonly Guid Referencia = Guid.Parse("dd000000-0000-4000-8000-000000000004");

    // Valores sonda: no pretenden ser realistas, pretenden ser inconfundibles en el JSON. Si alguno
    // aparece en la respuesta, la fuga esta ahi y no hay que interpretar nada.
    private const string TokenDeLaPasarela = "tok-sonda-no-debe-salir";
    private const string CodigoDeSeguridad = "sonda-cvv";
    private const string TitularDeLaTarjeta = "SONDA TITULAR";
    private const string Vencimiento = "2030/12";
    private const string DocumentoDelComprador = "1020304050";

    private const string NombreDelComprador = "Ana Restrepo";
    private const string UltimosCuatro = "4242";

    public PaymentSummaryDtoHasNoSecretsTest()
    {
        MapsterConfigPayment.Configure();
    }

    [Fact]
    public void ElContratoDeConsultaNoNombraNingunSecreto()
    {
        var encontrados = Recorrer(typeof(PaymentSummaryDto), nameof(PaymentSummaryDto), [])
            .Where(ruta => Prohibidos.Any(prohibido => ruta.Contains(prohibido, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(
            encontrados.Count == 0,
            $"El DTO de consulta expone datos que no pueden salir del microservicio: {string.Join(", ", encontrados)}");
    }

    [Fact]
    public void ElDtoInternoSiLosNombra()
    {
        // La prueba de la prueba. Si el recorrido no encontrara nada aqui —donde los secretos estan, y
        // anidados dos niveles bajo PaymentMethod— la comprobacion de arriba estaria pasando por vacia.
        var encontrados = Recorrer(typeof(PaymentDto), nameof(PaymentDto), [])
            .Where(ruta => Prohibidos.Any(prohibido => ruta.Contains(prohibido, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.Contains(encontrados, ruta => ruta.EndsWith("SecurityCode", StringComparison.Ordinal));
        Assert.Contains(encontrados, ruta => ruta.EndsWith("Token", StringComparison.Ordinal));
        Assert.Contains(encontrados, ruta => ruta.EndsWith("Document", StringComparison.Ordinal));
    }

    [Fact]
    public void LoQueSaleDeUnPagoConTarjetaEsLoJusto()
    {
        var resumen = Mapear(ConTarjeta());

        Assert.Equal(PaymentId, resumen.Id);
        Assert.Equal(Referencia, resumen.ReferenceId);
        Assert.Equal("licenses", resumen.Module);
        Assert.Equal(PaymentStatus.Initiated, resumen.Status);
        Assert.Equal(119_000L, resumen.Total.Amount);
        Assert.Equal(PaymentProvider.Payu, resumen.PaymentProvider);
        Assert.Equal(NombreDelComprador, resumen.BuyerName);

        // El tipo llega normalizado por el propio objeto de valor, que es lo que la pantalla muestra.
        Assert.Equal("VISA", resumen.PaymentMethodType);
        Assert.Equal(UltimosCuatro, resumen.Last4Digits);
    }

    [Fact]
    public void LaRespuestaSerializadaNoLleraNingunaSonda()
    {
        // El recorrido por reflexion mira la forma del tipo; esto mira lo que de verdad viaja. Son dos
        // fallos distintos: un campo nuevo cae en el primero, un serializador que decide publicar de mas
        // —o un tipo que se serializa entero— solo cae aqui.
        var json = JsonSerializer.Serialize(Mapear(ConTarjeta()));

        Assert.DoesNotContain(TokenDeLaPasarela, json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(CodigoDeSeguridad, json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(TitularDeLaTarjeta, json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Vencimiento, json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(DocumentoDelComprador, json, StringComparison.OrdinalIgnoreCase);

        // Y lo que si tiene que estar, para que la prueba no pase por servir una respuesta vacia.
        Assert.Contains(UltimosCuatro, json, StringComparison.Ordinal);
        Assert.Contains(NombreDelComprador, json, StringComparison.Ordinal);
    }

    [Fact]
    public void UnPagoPorPseNoInventaCuatroDigitos()
    {
        // PSE no tiene tarjeta detras. Devolver una cadena vacia haria que la pantalla pintase una casilla
        // de tarjeta vacia en vez de no pintarla.
        var resumen = Mapear(PorPse());

        Assert.Null(resumen.Last4Digits);
        Assert.Equal("PSE", resumen.PaymentMethodType);
    }

    /// <summary>
    /// Devuelve la ruta de cada propiedad alcanzable desde el tipo, entrando en los tipos anidados.
    /// </summary>
    /// <param name="tipo">El tipo a recorrer.</param>
    /// <param name="ruta">La ruta acumulada, que es lo que se reporta cuando algo cae.</param>
    /// <param name="visitados">Los tipos ya recorridos, para no girar en un ciclo de referencias.</param>
    private static IEnumerable<string> Recorrer(Type tipo, string ruta, HashSet<Type> visitados)
    {
        tipo = Nullable.GetUnderlyingType(tipo) ?? tipo;

        if (EsHoja(tipo) || !visitados.Add(tipo))
            yield break;

        // Una coleccion no aporta nombres propios: lo que importa es de que esta hecha.
        foreach (var elemento in Elementos(tipo))
        {
            foreach (var anidada in Recorrer(elemento, $"{ruta}[]", visitados))
                yield return anidada;
        }

        foreach (var propiedad in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var rutaPropiedad = $"{ruta}.{propiedad.Name}";

            yield return rutaPropiedad;

            foreach (var anidada in Recorrer(propiedad.PropertyType, rutaPropiedad, visitados))
                yield return anidada;
        }
    }

    private static IEnumerable<Type> Elementos(Type tipo)
    {
        if (!typeof(IEnumerable).IsAssignableFrom(tipo))
            return [];

        if (tipo.IsArray)
            return [tipo.GetElementType()!];

        return tipo.IsGenericType ? tipo.GetGenericArguments() : [];
    }

    /// <summary>
    /// Decide donde se detiene el recorrido.
    /// </summary>
    /// <remarks>
    /// NodaTime se corta entero: sus tipos son estructuras con decenas de propiedades que se referencian
    /// entre si, y ninguna puede llamarse como un secreto nuestro.
    /// </remarks>
    private static bool EsHoja(Type tipo) =>
        tipo.IsPrimitive
        || tipo.IsEnum
        || tipo == typeof(string)
        || tipo == typeof(decimal)
        || tipo == typeof(Guid)
        || tipo == typeof(DateTime)
        || tipo == typeof(DateTimeOffset)
        || tipo == typeof(TimeSpan)
        || tipo == typeof(object)
        || tipo.Namespace is null
        || tipo.Namespace.StartsWith("NodaTime", StringComparison.Ordinal)
        || tipo.Namespace.StartsWith("System.Reflection", StringComparison.Ordinal);

    private static PaymentSummaryDto Mapear(PaymentAggregate pago) => pago.Adapt<PaymentSummaryDto>();

    private static PaymentAggregate ConTarjeta() => Construir(
        MetodoDePago.Create("VISA", null, Tarjeta.Create(TokenDeLaPasarela, UltimosCuatro, Vencimiento, TitularDeLaTarjeta, CodigoDeSeguridad)));

    private static PaymentAggregate PorPse() => Construir(
        MetodoDePago.Create("PSE", TransferenciaPse.Create("1022", "N", "https://kappali.co/pse/respuesta"), null));

    private static PaymentAggregate Construir(MetodoDePago metodoDePago)
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");

        var comprador = Buyer.CreateWithoutShipping(
            Comprador, NombreDelComprador, "+573001112233", "ana@example.com",
            TypeDocument.Create("CC", "Cedula de ciudadania"), DocumentoDelComprador);

        return PaymentAggregate.Create(
            PaymentId, "licenses", Referencia, subTotal, tax, subTotal + tax, comprador, null,
            metodoDePago, "Compra de licencia", PaymentProvider.Payu, Tenant, Comprador);
    }
}
