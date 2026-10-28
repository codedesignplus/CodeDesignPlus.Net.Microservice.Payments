using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeDesignPlus.Net.Microservice.Payments.Application.Setup;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.DataTransferObjects;

/// <summary>
/// Guards that the payments query contract does not publish what it must not.
/// </summary>
/// <remarks>
/// The aggregate keeps the buyer's document and the raw answers of the provider. None of that may leave by REST, and
/// the only reason it does not is that the DTO was written with care. That gets forgotten, so the type is walked by
/// reflection against a list of forbidden names, nested types included.
/// </remarks>
public class PaymentSummaryDtoHasNoSecretsTest
{
    /// <summary>
    /// Names that may not appear anywhere in the contract.
    /// </summary>
    /// <remarks>
    /// The first ones identify a person or would let someone charge them. The last two are the provider's raw answers:
    /// their content is decided by the provider, so publishing them is signing a blank cheque.
    /// </remarks>
    private static readonly string[] Forbidden =
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
    private static readonly Guid Buyer = Guid.Parse("cc000000-0000-4000-8000-000000000003");
    private static readonly Guid Reference = Guid.Parse("dd000000-0000-4000-8000-000000000004");

    // Probe values: not realistic, unmistakable in the JSON. If one shows up in the answer, the leak is there.
    private const string BuyerDocumentProbe = "1020304050";
    private const string ProviderAnswerProbe = "provider-answer-probe";
    private const string BuyerName = "Ana Restrepo";

    public PaymentSummaryDtoHasNoSecretsTest()
    {
        MapsterConfigPayment.Configure();
    }

    [Fact]
    public void PaymentSummaryDto_WalkedByReflection_NamesNoSecret()
    {
        // Arrange
        var paths = Walk(typeof(PaymentSummaryDto), nameof(PaymentSummaryDto), []);

        // Act
        var found = paths.Where(path => Forbidden.Any(name => path.Contains(name, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        Assert.True(found.Count == 0, $"The query DTO exposes data that may not leave the service: {string.Join(", ", found)}");
    }

    [Fact]
    public void PaymentDto_WalkedByReflection_NamesTheProviderAnswers()
    {
        // Arrange: the test of the test. If the walk found nothing here, where the raw answers are, the check above
        // would be passing on empty.
        var paths = Walk(typeof(PaymentDto), nameof(PaymentDto), []);

        // Act
        var found = paths.Where(path => Forbidden.Any(name => path.Contains(name, StringComparison.OrdinalIgnoreCase))).ToList();

        // Assert
        Assert.Contains(found, path => path.EndsWith("FinalResponse", StringComparison.Ordinal));
    }

    [Fact]
    public void Adapt_SucceededPayment_KeepsWhatTheScreenShows()
    {
        // Arrange
        var payment = Succeeded();

        // Act
        var summary = payment.Adapt<PaymentSummaryDto>();

        // Assert
        Assert.Equal((PaymentId, Reference, "licenses", PaymentStatus.Succeeded, 119_000L, "Wompi", BuyerName, "CARD"), (summary.Id, summary.ReferenceId, summary.Module, summary.Status, summary.Total.Amount, summary.PaymentProvider, summary.BuyerName, summary.PaymentMethodType));
    }

    [Fact]
    public void Serialize_SucceededPaymentSummary_CarriesNoProbe()
    {
        // Arrange: reflection looks at the shape; this looks at what actually travels.
        var summary = Succeeded().Adapt<PaymentSummaryDto>();

        // Act
        var json = JsonSerializer.Serialize(summary);

        // Assert
        Assert.False(json.Contains(BuyerDocumentProbe, StringComparison.Ordinal) || json.Contains(ProviderAnswerProbe, StringComparison.Ordinal) || !json.Contains(BuyerName, StringComparison.Ordinal), json);
    }

    [Fact]
    public void Adapt_PaymentInProgress_HasNoPaymentMethodYet()
    {
        // Arrange: the payer chooses the method on the provider's page, so nothing is known until the provider answers.
        var payment = Build();

        // Act
        var summary = payment.Adapt<PaymentSummaryDto>();

        // Assert
        Assert.Null(summary.PaymentMethodType);
    }

    private static IEnumerable<string> Walk(Type type, string path, HashSet<Type> visited)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (IsLeaf(type) || !visited.Add(type))
        {
            yield break;
        }

        foreach (var element in Elements(type))
        {
            foreach (var nested in Walk(element, $"{path}[]", visited))
            {
                yield return nested;
            }
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertyPath = $"{path}.{property.Name}";

            yield return propertyPath;

            foreach (var nested in Walk(property.PropertyType, propertyPath, visited))
            {
                yield return nested;
            }
        }
    }

    private static IEnumerable<Type> Elements(Type type)
    {
        if (!typeof(IEnumerable).IsAssignableFrom(type))
        {
            return [];
        }

        if (type.IsArray)
        {
            return [type.GetElementType()!];
        }

        return type.IsGenericType ? type.GetGenericArguments() : [];
    }

    private static bool IsLeaf(Type type) =>
        type.IsPrimitive
        || type.IsEnum
        || type == typeof(string)
        || type == typeof(decimal)
        || type == typeof(Guid)
        || type == typeof(DateTime)
        || type == typeof(DateTimeOffset)
        || type == typeof(TimeSpan)
        || type == typeof(object)
        || type.Namespace is null
        || type.Namespace.StartsWith("NodaTime", StringComparison.Ordinal)
        || type.Namespace.StartsWith("System.Reflection", StringComparison.Ordinal);

    private static PaymentAggregate Succeeded()
    {
        var payment = Build();

        payment.SetInitiateResponse(new Dictionary<string, string?> { ["redirectUrl"] = ProviderAnswerProbe });
        payment.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["statusMessage"] = ProviderAnswerProbe }, "tx-1", payment.Total, "CARD");

        return payment;
    }

    private static PaymentAggregate Build()
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");

        var buyer = Net.ValueObjects.User.Buyer.CreateWithoutShipping(Buyer, BuyerName, "+573001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), BuyerDocumentProbe);

        return PaymentAggregate.Create(PaymentId, "licenses", Reference, subTotal, tax, subTotal + tax, buyer, "Compra de licencia", "Wompi", false, Tenant, Buyer);
    }
}
