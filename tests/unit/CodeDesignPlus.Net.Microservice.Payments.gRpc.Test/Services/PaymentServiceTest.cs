using System.Reflection;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;
using CodeDesignPlus.Net.Microservice.Payments.gRpc;
using Microsoft.AspNetCore.Authorization;

namespace CodeDesignPlus.Net.Microservice.Payments.gRpc.Test.Services;

/// <summary>
/// The contract with the modules that charge and pay through this service.
/// </summary>
public class PaymentServiceTest
{
    public PaymentServiceTest()
    {
        CodeDesignPlus.Net.Microservice.Payments.gRpc.Core.Mapster.MapsterConfig.Configure();
    }

    [Theory]
    [InlineData(nameof(PaymentService.GetMerchantAccount))]
    [InlineData(nameof(PaymentService.RequestPayout))]
    [InlineData(nameof(PaymentService.GetPayout))]
    public void WorkerMethods_AreAnonymous(string method)
    {
        // Arrange: a worker calls them without a user token; the tenant travels in the request.
        var info = typeof(PaymentService).GetMethod(method)!;

        // Act
        var anonymous = info.GetCustomAttribute<AllowAnonymousAttribute>();

        // Assert
        Assert.NotNull(anonymous);
    }

    [Theory]
    [InlineData(nameof(PaymentService.InitiatePayment))]
    [InlineData(nameof(PaymentService.GetPaymentStatus))]
    public void UserMethods_StayAuthenticated(string method)
    {
        // Arrange
        var info = typeof(PaymentService).GetMethod(method)!;

        // Act
        var anonymous = info.GetCustomAttribute<AllowAnonymousAttribute>();

        // Assert
        Assert.Null(anonymous);
    }

    [Fact]
    public void Adapt_RequestFromAnUpgradedCaller_CarriesProviderReturnUrlAndPlatformAccount()
    {
        // Arrange
        var request = Request();
        request.Provider = "Wompi";
        request.ReturnUrl = "https://www.example.com/return";
        request.PlatformAccount = true;

        // Act
        var command = request.Adapt<InitiatePaymentCommand>();

        // Assert
        Assert.Equal(("Wompi", "https://www.example.com/return", true), (command.Provider, command.ReturnUrl, command.PlatformAccount));
    }

    [Fact]
    public void Adapt_RequestWithoutProvider_LeavesItToTheAccount()
    {
        // Act
        var command = Request().Adapt<InitiatePaymentCommand>();

        // Assert
        Assert.Equal(((string?)null, (string?)null, false), (command.Provider, command.ReturnUrl, command.PlatformAccount));
    }

    [Fact]
    public void Adapt_BuyerWithoutDocument_IsAccepted()
    {
        // Arrange: the hosted page asks the payer for what the caller did not send.
        var request = Request();
        request.Buyer.ClearDocument();
        request.Buyer.TypeDocument = null;

        // Act
        var command = request.Adapt<InitiatePaymentCommand>();

        // Assert
        Assert.Null(command.Buyer.Document);
    }

    private static InitiatePaymentRequest Request() => new()
    {
        Id = Guid.NewGuid().ToString(),
        Module = "Invoicing",
        ReferenceId = Guid.NewGuid().ToString(),
        SubTotal = new Amount { Value = 100_000L, Currency = "COP" },
        Tax = new Amount { Value = 19_000L, Currency = "COP" },
        Total = new Amount { Value = 119_000L, Currency = "COP" },
        Description = "Cuota",
        Buyer = new Buyer
        {
            BuyerId = Guid.NewGuid().ToString(),
            Name = "Ana Restrepo",
            Phone = "3001112233",
            Email = "ana@example.com",
            TypeDocument = new TypeDocument { Code = "CC", Name = "Cedula" },
            Document = "1020304050"
        }
    };
}
