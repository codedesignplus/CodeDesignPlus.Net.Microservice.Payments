using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Accounts;
using CodeDesignPlus.Net.Microservice.Payments.Application.Common.Options;
using CodeDesignPlus.Net.Microservice.Payments.Application.MerchantAccount.Commands;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Vault;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Wompi;
using CodeDesignPlus.Net.Resilience.Extensions;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure;

public class Startup : IStartup
{
    public void Initialize(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<PaymentProvidersOptions>()
            .Bind(configuration.GetSection(PaymentProvidersOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var wompiSection = configuration.GetSection(WompiOptions.Section);

        services
            .AddOptions<WompiOptions>()
            .Bind(wompiSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddResilience();

        services
            .AddHttpClient(WompiClient.HttpClientName)
            .AddResiliencePolicies(wompiSection.Get<WompiOptions>()?.Resilience ?? new());

        services.AddSingleton<IWompiClient, WompiClient>();
        services.AddSingleton<WompiProvider>();
        services.AddSingleton<ICollectProvider>(x => x.GetRequiredService<WompiProvider>());
        services.AddSingleton<IMerchantCredentialsProvider>(x => x.GetRequiredService<WompiProvider>());
        services.AddSingleton<IPayoutProvider>(x => x.GetRequiredService<WompiProvider>());
        services.AddSingleton<IPlatformMerchantAccount, WompiPlatformAccount>();

        services.AddSingleton<IPaymentProviderFactory, PaymentProviderFactory>();
        services.AddSingleton<IMerchantSecretStore, VaultMerchantSecretStore>();
        services.AddScoped<IMerchantCredentialsResolver, MerchantCredentialsResolver>();
        services.AddScoped<IMerchantAccountConnector, MerchantAccountConnector>();
    }
}
