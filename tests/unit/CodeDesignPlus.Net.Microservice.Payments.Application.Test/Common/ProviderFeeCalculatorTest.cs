using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Fees;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;
using CodeDesignPlus.Net.ValueObjects.Financial;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;

/// <summary>
/// The commission comes from the tenant's rates, from the provider's default when it set none, and from the provider when
/// it reports it.
/// </summary>
public class ProviderFeeCalculatorTest
{
    private static readonly Money Gross = Money.FromLong(1_000_000L, "COP");

    private readonly Mock<IMerchantAccountRepository> accounts = new();

    [Fact]
    public async Task ForAsync_AccountWithoutRates_UsesTheDefaultOfTheProvider()
    {
        // Arrange
        accounts.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.ActiveAccount());

        // Act
        var fee = await Calculator().ForAsync(TestData.Payment(), Gross, "PSE", null, CancellationToken.None);

        // Assert
        Assert.Equal((96_500L, 18_335L, ProviderFeeSources.Calculated), (fee!.Fee.Amount, fee.Tax.Amount, fee.Source));
    }

    [Fact]
    public async Task ForAsync_AccountWithItsOwnRates_UsesThem()
    {
        // Arrange
        var account = TestData.ActiveAccount();

        account.SetFeeSchedule(ProviderFeeSchedule.Create(ProviderFeeRate.Create(199, Money.FromLong(50_000L, "COP"), 1900), null), TestData.User);
        accounts.Setup(x => x.GetByTenantAsync(TestData.Tenant, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        // Act
        var fee = await Calculator().ForAsync(TestData.Payment(), Gross, "PSE", null, CancellationToken.None);

        // Assert
        Assert.Equal((69_900L, 13_281L), (fee!.Fee.Amount, fee.Tax.Amount));
    }

    [Fact]
    public async Task ForAsync_PlatformAccount_UsesTheDefaultWithoutLookingForATenantAccount()
    {
        // Act
        var fee = await Calculator().ForAsync(TestData.Payment(platformAccount: true), Gross, "CARD", null, CancellationToken.None);

        // Assert
        Assert.Equal(96_500L, fee!.Fee.Amount);
        accounts.Verify(x => x.GetByTenantAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForAsync_ProviderReportsIt_TheReportedOnePrevails()
    {
        // Arrange
        var reported = new ProviderFee(Money.FromLong(96_000L, "COP"), Money.FromLong(18_240L, "COP"), ProviderFeeSources.Calculated);

        // Act
        var fee = await Calculator().ForAsync(TestData.Payment(platformAccount: true), Gross, "PSE", reported, CancellationToken.None);

        // Assert
        Assert.Equal((96_000L, 18_240L, ProviderFeeSources.Reported), (fee!.Fee.Amount, fee.Tax.Amount, fee.Source));
    }

    [Fact]
    public async Task ForAsync_NoRatesForTheProvider_IsUnknown()
    {
        // Act
        var fee = await Calculator(withDefault: false).ForAsync(TestData.Payment(platformAccount: true), Gross, "PSE", null, CancellationToken.None);

        // Assert
        Assert.Null(fee);
    }

    private ProviderFeeCalculator Calculator(bool withDefault = true)
    {
        var options = new PaymentProvidersOptions();

        if (withDefault)
        {
            options.DefaultFees["Wompi"] = new ProviderFeeScheduleOptions
            {
                Default = new ProviderFeeRateOptions { RateBasisPoints = 265, FixedAmount = 70_000L, Currency = "COP", TaxRateBasisPoints = 1900 }
            };
        }

        return new ProviderFeeCalculator(accounts.Object, Options.Create(options), Mock.Of<ILogger<ProviderFeeCalculator>>());
    }
}
