using CodeDesignPlus.Net.Core.Abstractions.Models.Pager;
using CodeDesignPlus.Net.Criteria.Extensions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetAllPlatformPayments;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Queries.GetPlatformPaymentById;
using CodeDesignPlus.Net.Microservice.Payments.Application.Setup;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Queries.PlatformPayments;

/// <summary>
/// The Super Admin reads the payments of the platform's own merchant account, with what the provider kept (pendings/674).
/// </summary>
/// <remarks>
/// The commission of the purchase of Las Acacias ($21.197,75 plus $4.027,57 of tax on $773.500) was only in Mongo.
/// </remarks>
public class PlatformPaymentsQueryTest
{
    private readonly Mock<IPaymentRepository> repository = new();

    public PlatformPaymentsQueryTest()
    {
        MapsterConfigPayment.Configure();
    }

    [Fact]
    public async Task GetAll_ClientFilter_KeepsItAndAddsThePlatformAccountLast()
    {
        // Arrange
        C.Criteria? sent = null;
        repository.Setup(x => x.MatchingAsync<PaymentAggregate>(It.IsAny<C.Criteria>(), It.IsAny<CancellationToken>()))
                  .Callback<C.Criteria, CancellationToken>((criteria, _) => sent = criteria)
                  .ReturnsAsync(Pagination<PaymentAggregate>.Create([], 0, 10, 0));

        // Act
        await Handler().Handle(new GetAllPlatformPaymentsQuery(new C.Criteria { Filters = "Status=2|or|Status=3", OrderBy = "CreatedAt", Limit = 25 }), CancellationToken.None);

        // Assert: grouped left to right, so the client's «or» cannot let a tenant's payment through.
        Assert.Equal(("Status=2|or|Status=3|and|PlatformAccount=true", "CreatedAt", 25), (sent!.Filters, sent.OrderBy, sent.Limit));
    }

    [Fact]
    public async Task GetAll_WithoutFilter_OnlyAsksForThePlatformAccount()
    {
        // Arrange
        C.Criteria? sent = null;
        repository.Setup(x => x.MatchingAsync<PaymentAggregate>(It.IsAny<C.Criteria>(), It.IsAny<CancellationToken>()))
                  .Callback<C.Criteria, CancellationToken>((criteria, _) => sent = criteria)
                  .ReturnsAsync(Pagination<PaymentAggregate>.Create([], 0, 10, 0));

        // Act
        await Handler().Handle(new GetAllPlatformPaymentsQuery(new C.Criteria()), CancellationToken.None);

        // Assert
        Assert.Equal("PlatformAccount=true", sent!.Filters);
    }

    [Fact]
    public void Scope_ClientAsksForEverything_LetsThroughOnlyThePlatformPayments()
    {
        // Arrange: the filter the SDK evaluates against the stored payments.
        var filter = Application.Payment.PlatformScopedCriteria.Scope(new C.Criteria { Filters = "PlatformAccount=false|or|Module=Invoicing" }).GetFilterExpression<PaymentAggregate>().Compile();

        // Act
        var passes = (filter(TestData.Payment(platformAccount: true)), filter(TestData.Payment()));

        // Assert
        Assert.Equal((true, false), passes);
    }

    [Fact]
    public async Task GetById_ApprovedLicense_CarriesGrossCommissionTaxAndNet()
    {
        // Arrange
        var payment = Approved();
        repository.Setup(x => x.FindAsync<PaymentAggregate>(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        // Act
        var dto = await ByIdHandler().Handle(new GetPlatformPaymentByIdQuery(payment.Id), CancellationToken.None);

        // Assert
        Assert.Equal((77_350_000L, 2_119_775L, 402_757L, 74_827_468L), (dto.CapturedAmount!.Amount, dto.ProviderFee!.Amount, dto.ProviderFeeTax!.Amount, dto.NetAmount!.Amount));
    }

    [Fact]
    public async Task GetById_ApprovedLicense_CarriesProviderMethodTransactionAndOrder()
    {
        // Arrange
        var payment = Approved();
        repository.Setup(x => x.FindAsync<PaymentAggregate>(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        // Act
        var dto = await ByIdHandler().Handle(new GetPlatformPaymentByIdQuery(payment.Id), CancellationToken.None);

        // Assert
        Assert.Equal(("Wompi", "CARD", "12239445-1793302711-61592", PaymentStatus.Succeeded, payment.ReferenceId), (dto.Provider, dto.PaymentMethodType, dto.ProviderTransactionId, dto.Status, dto.ReferenceId));
    }

    [Fact]
    public async Task GetById_PaymentStillOpen_HasNoNet()
    {
        // Arrange
        var payment = TestData.Payment(platformAccount: true);
        repository.Setup(x => x.FindAsync<PaymentAggregate>(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        // Act
        var dto = await ByIdHandler().Handle(new GetPlatformPaymentByIdQuery(payment.Id), CancellationToken.None);

        // Assert
        Assert.Null(dto.NetAmount);
    }

    [Fact]
    public async Task GetById_TenantPayment_AnswersAsNotFound()
    {
        // Arrange
        var payment = TestData.Payment();
        repository.Setup(x => x.FindAsync<PaymentAggregate>(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        // Act
        var error = await Assert.ThrowsAsync<CodeDesignPlusException>(() => ByIdHandler().Handle(new GetPlatformPaymentByIdQuery(payment.Id), CancellationToken.None));

        // Assert
        Assert.Equal(Errors.PaymentNotFound.Code, error.Code);
    }

    private static PaymentAggregate Approved()
    {
        var subTotal = Money.FromLong(65_000_000L, "COP");
        var tax = Money.FromLong(12_350_000L, "COP");
        var payment = TestData.Payment(platformAccount: true);
        var buyer = payment.Buyer;
        var license = PaymentAggregate.Create(Guid.NewGuid(), PaymentAggregate.LicensesModule, Guid.NewGuid(), subTotal, tax, subTotal + tax, buyer, "Licencia Estándar", "Wompi", true, null, TestData.User);

        license.SetFinalResponse(PaymentStatus.Succeeded, new Dictionary<string, string?> { ["status"] = "APPROVED" }, "12239445-1793302711-61592", subTotal + tax, "CARD", new ProviderFee(Money.FromLong(2_119_775L, "COP"), Money.FromLong(402_757L, "COP"), ProviderFeeSources.Reported));

        return license;
    }

    private GetAllPlatformPaymentsQueryHandler Handler() => new(repository.Object, new Mapper(TypeAdapterConfig.GlobalSettings));

    private GetPlatformPaymentByIdQueryHandler ByIdHandler() => new(repository.Object, new Mapper(TypeAdapterConfig.GlobalSettings));
}
