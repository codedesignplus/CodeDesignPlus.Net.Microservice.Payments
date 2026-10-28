using System.ComponentModel.DataAnnotations;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services;

/// <summary>
/// The settings shipped with each entrypoint pass the validation that runs on start: a section that does not bind
/// would leave the pod restarting after the deployment.
/// </summary>
public class ShippedSettingsTest
{
    public static TheoryData<string> Entrypoints() => new()
    {
        "CodeDesignPlus.Net.Microservice.Payments.Rest",
        "CodeDesignPlus.Net.Microservice.Payments.gRpc",
        "CodeDesignPlus.Net.Microservice.Payments.AsyncWorker"
    };

    [Theory]
    [MemberData(nameof(Entrypoints))]
    public void PaymentProviders_ShippedSettings_AreValidAndRouteColombiaToWompi(string entrypoint)
    {
        // Arrange
        var options = Settings(entrypoint).GetSection(PaymentProvidersOptions.Section).Get<PaymentProvidersOptions>()!;

        // Act
        var errors = Validate(options);

        // Assert
        Assert.Equal((0, "Wompi", MerchantAccountEnvironment.Sandbox), (errors.Count, options.ProviderFor("CO"), options.Environment));
    }

    [Theory]
    [MemberData(nameof(Entrypoints))]
    public void Wompi_ShippedSettings_AreValidAndCarryNoKey(string entrypoint)
    {
        // Arrange: the platform keys come from the Vault, never from a file in the repository.
        var options = Settings(entrypoint).GetSection(WompiOptions.Section).Get<WompiOptions>()!;

        // Act
        var errors = Validate(options);

        // Assert
        Assert.Equal((0, true), (errors.Count, string.IsNullOrEmpty(options.Platform.PrivateKey) && string.IsNullOrEmpty(options.Platform.PublicKey)));
    }

    private static List<ValidationResult> Validate(object options)
    {
        var errors = new List<ValidationResult>();

        Validator.TryValidateObject(options, new ValidationContext(options), errors, validateAllProperties: true);

        return errors;
    }

    private static IConfiguration Settings(string entrypoint)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        var file = Path.Combine(directory!.FullName, "src", "entrypoints", entrypoint, "appsettings.json");

        return new ConfigurationBuilder().AddJsonFile(file).Build();
    }
}
