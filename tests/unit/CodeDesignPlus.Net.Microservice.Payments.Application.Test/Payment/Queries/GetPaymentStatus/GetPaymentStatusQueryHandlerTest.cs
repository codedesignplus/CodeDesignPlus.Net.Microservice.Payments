using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPaymentStatus;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.ValueObjects.Financial;
using CodeDesignPlus.Net.ValueObjects.User;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Queries.GetPaymentStatus;

/// <summary>
/// The status is read by a user of a tenant and, without a token, by the order reconciliation job of ms-licenses
/// (pendings/738).
/// </summary>
public class GetPaymentStatusQueryHandlerTest
{
    private static readonly Guid PaymentId = Guid.Parse("aa000000-0000-4000-8000-000000000011");
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000012");

    private readonly Mock<IPaymentRepository> repository = new();
    private readonly Mock<IUserContext> user = new();

    [Fact]
    public async Task Handle_CallWithoutUser_FindsThePaymentByItsIdAlone()
    {
        // Arrange: an anonymous gRPC call carries no tenant.
        user.SetupGet(x => x.Tenant).Returns(Guid.Empty);
        repository.Setup(x => x.FindAsync<PaymentAggregate>(PaymentId, It.IsAny<CancellationToken>())).ReturnsAsync(Initiated(null));

        // Act
        var status = await Handler().Handle(new GetPaymentStatusQuery(PaymentId), CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Initiated, status.Status);
    }

    [Fact]
    public async Task Handle_UserOfATenant_FindsThePaymentWithinItsTenant()
    {
        // Arrange
        user.SetupGet(x => x.Tenant).Returns(Tenant);
        repository.Setup(x => x.FindAsync<PaymentAggregate>(PaymentId, Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(Initiated(Tenant));

        // Act
        var status = await Handler().Handle(new GetPaymentStatusQuery(PaymentId), CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Initiated, status.Status);
        repository.Verify(x => x.FindAsync<PaymentAggregate>(PaymentId, It.IsAny<CancellationToken>()), Times.Never);
    }

    private GetPaymentStatusQueryHandler Handler() => new(repository.Object, user.Object);

    private static PaymentAggregate Initiated(Guid? tenant)
    {
        var subTotal = Money.FromLong(100_000L, "COP");
        var tax = Money.FromLong(19_000L, "COP");
        var buyer = Buyer.CreateWithoutShipping(Guid.NewGuid(), "Ana Restrepo", "3001112233", "ana@example.com", TypeDocument.Create("CC", "Cedula de ciudadania"), "1020304050");

        return PaymentAggregate.Create(PaymentId, PaymentAggregate.LicensesModule, Guid.NewGuid(), subTotal, tax, subTotal + tax, buyer, "Licencia", "Wompi", true, tenant, Guid.NewGuid());
    }
}
