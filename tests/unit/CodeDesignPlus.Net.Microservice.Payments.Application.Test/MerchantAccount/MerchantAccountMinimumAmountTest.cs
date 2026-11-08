using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.DataTransferObjects;
using CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.MerchantAccount;

/// <summary>
/// The collection account tells the front desk the smallest amount its provider accepts online, so the screen offers the QR
/// code only when it can be paid (pendings/665).
/// </summary>
public class MerchantAccountMinimumAmountTest
{
    private static readonly PaymentProvidersOptions Options = new()
    {
        DefaultReturnUrl = new Uri("https://www.example.com/return"),
        EventsUrl = new Uri("https://www.example.com/notify/wompi"),
        MinimumAmounts = new() { ["Wompi"] = new() { ["COP"] = 150_000L } },
    };

    [Fact]
    public void From_WompiAccount_CarriesItsMinimum()
    {
        // Act
        var dto = MerchantAccountDto.From(TestData.ActiveAccount(), Options);

        // Assert
        Assert.Equal((150_000L, "COP"), (Assert.Single(dto.MinimumAmounts).Amount, dto.MinimumAmounts[0].Currency));
    }

    [Fact]
    public void Pending_WompiAccount_CarriesItsMinimum()
    {
        // Act
        var dto = MerchantAccountDto.Pending(TestData.Tenant, "Wompi", Options);

        // Assert
        Assert.Equal(150_000L, Assert.Single(dto.MinimumAmounts).Amount);
    }

    [Fact]
    public void From_ProviderWithoutMinimum_CarriesNone()
    {
        // Act
        var dto = MerchantAccountDto.Pending(TestData.Tenant, "OtherProvider", Options);

        // Assert
        Assert.Empty(dto.MinimumAmounts);
    }
}
