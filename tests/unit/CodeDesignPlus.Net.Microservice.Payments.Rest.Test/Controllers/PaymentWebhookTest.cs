using CodeDesignPlus.Net.Exceptions;
using CodeDesignPlus.Net.Exceptions.Extensions;
using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.ProcessProviderEvent;
using CodeDesignPlus.Net.Microservice.Payments.Rest.Controllers;
using Microsoft.AspNetCore.Http;
using InfraErrors = CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Rest.Test.Controllers;

/// <summary>
/// The webhook answers 503 when a dependency did not answer, so the provider retries the event instead of losing it.
/// </summary>
public class PaymentWebhookTest
{
    private readonly Mock<IMediator> mediator = new();

    [Fact]
    public async Task Notify_VaultDoesNotAnswer_Answers503()
    {
        // Arrange
        Throws(InfraErrors.MerchantSecretsVaultIsUnavailable);

        // Act
        var result = await Notify();

        // Assert
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }

    [Fact]
    public async Task Notify_ProviderDoesNotAnswer_Answers503()
    {
        // Arrange
        Throws(InfraErrors.PaymentProviderIsUnavailable);

        // Act
        var result = await Notify();

        // Assert
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, Assert.IsType<StatusCodeResult>(result).StatusCode);
    }

    [Fact]
    public async Task Notify_EventProcessed_Answers200()
    {
        // Act
        var result = await Notify();

        // Assert
        Assert.IsType<OkResult>(result);
    }

    private void Throws(Error error)
    {
        mediator.Setup(x => x.Send(It.IsAny<ProcessProviderEventCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new CodeDesignPlusException(Layer.Infrastructure, error.GetCode(), error.GetMessage()));
    }

    private Task<IActionResult> Notify()
    {
        var controller = new PaymentController(mediator.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        controller.HttpContext.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"event\":\"transaction.updated\"}"));

        return controller.Notify("wompi", CancellationToken.None);
    }
}
