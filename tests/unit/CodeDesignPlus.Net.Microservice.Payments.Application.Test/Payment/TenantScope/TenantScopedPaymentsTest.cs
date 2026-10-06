using CodeDesignPlus.Net.Criteria.Extensions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
// The microservice namespace has a "Payment" segment that wins over the using directives.
using PaymentMethodValue = CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod;
using CreditCardValue = CodeDesignPlus.Net.ValueObjects.Payment.CreditCard;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.TenantScope;

/// <summary>
/// The payments screen of a property only shows that property's payments (pendings/295).
/// </summary>
/// <remarks>
/// The filter is evaluated with the SDK's own criteria parser against in-memory payments, because the repository does
/// not apply the tenant to <see cref="PaymentAggregate"/>: a test with a mocked repository passed while production
/// listed every property's payments.
/// </remarks>
public class TenantScopedPaymentsTest
{
    private static readonly Guid TenantA = Guid.Parse("aa000000-0000-4000-8000-0000000000a1");
    private static readonly Guid TenantB = Guid.Parse("bb000000-0000-4000-8000-0000000000b2");

    [Fact]
    public void Scope_KeepsOnlyThePaymentsOfTheSessionProperty()
    {
        var payments = new[]
        {
            Create("Parking", TenantA),
            Create("Parking", TenantB),
            Create(PaymentAggregate.LicensesModule, TenantA),
            Create(PaymentAggregate.LicensesModule, Guid.Empty),
        };

        var visible = Apply(TenantScopedCriteria.Scope(TenantA, new C.Criteria()), payments);

        var payment = Assert.Single(visible);
        Assert.Equal(TenantA, payment.Tenant);
        Assert.Equal("Parking", payment.Module);
    }

    [Fact]
    public void Scope_ClientOrFilterCannotEscapeTheProperty()
    {
        var payments = new[] { Create("Parking", TenantA), Create("Parking", TenantB) };

        var criteria = new C.Criteria { Filters = "module=Parking|or|module=Licenses" };
        var visible = Apply(TenantScopedCriteria.Scope(TenantA, criteria), payments);

        Assert.All(visible, x => Assert.Equal(TenantA, x.Tenant));
        Assert.Single(visible);
    }

    [Fact]
    public void Scope_WithoutPropertyIsRejected()
    {
        var exception = Assert.Throws<CodeDesignPlusException>(() => TenantScopedCriteria.Scope(Guid.Empty, new C.Criteria()));

        Assert.Equal(Application.Errors.TenantIsRequiredToListPayments.GetCode(), exception.Code);
    }

    [Fact]
    public void Scope_KeepsPagingAndOrder()
    {
        var criteria = new C.Criteria { Filters = "module=Parking", OrderBy = "createdAt", Skip = 20, Limit = 10 };

        var scoped = TenantScopedCriteria.Scope(TenantA, criteria);

        Assert.Equal($"module=Parking|and|tenant={TenantA}", scoped.Filters);
        Assert.Equal("createdAt", scoped.OrderBy);
        Assert.Equal(20, scoped.Skip);
        Assert.Equal(10, scoped.Limit);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_LicensePaymentNeverKeepsTheBuyersOpenProperty(bool emptyTenant)
    {
        var payment = Create(PaymentAggregate.LicensesModule, emptyTenant ? Guid.Empty : TenantA);

        Assert.Null(payment.Tenant);
    }

    [Fact]
    public void Create_PropertyPaymentKeepsItsProperty()
    {
        Assert.Equal(TenantA, Create("Parking", TenantA).Tenant);
    }

    private static List<PaymentAggregate> Apply(C.Criteria criteria, IEnumerable<PaymentAggregate> payments)
        => payments.AsQueryable().Where(criteria.GetFilterExpression<PaymentAggregate>()).ToList();

    private static PaymentAggregate Create(string module, Guid tenant)
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");
        var buyerId = Guid.NewGuid();
        var buyer = Buyer.CreateWithoutShipping(buyerId, "Ana Restrepo", "+573001112233", "ana@example.com",
            TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        return PaymentAggregate.Create(
            Guid.NewGuid(), module, Guid.NewGuid(), subTotal, tax, subTotal + tax, buyer, null,
            PaymentMethodValue.Create("VISA", null, CreditCardValue.Create("tok", "4242", "2030/12", "HOLDER", "cvv")),
            "Payment", PaymentProvider.Payu, tenant, buyerId);
    }
}
