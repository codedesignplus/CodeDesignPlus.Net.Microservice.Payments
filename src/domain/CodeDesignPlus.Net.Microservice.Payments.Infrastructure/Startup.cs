using CodeDesignPlus.Net.Microservice.Payments.Application.Common;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Enums;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure
{
    public class Startup : IStartup
    {
        public void Initialize(IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection(PayuOptions.Section);

            services
                  .AddOptions<PayuOptions>()
                  .Bind(section)
                  .ValidateDataAnnotations()
                  .ValidateOnStart();

            services.AddScoped<IPayu, PayUAdapter>();
            services.AddKeyedScoped<IPaymentProviderAdapter, PayUAdapter>(PaymentProvider.Payu);
            services.AddScoped<IPaymentProviderAdapterFactory, PaymentProviderAdapterFactory>();

            var payuOptions = section.Get<PayuOptions>();

            if (payuOptions != null && payuOptions.Enable)
            {
                services.AddScoped<IPayu, PayUAdapter>();

                var httpClientBuilder = services.AddHttpClient("Payu", client =>
                {
                    client.BaseAddress = payuOptions.Url;
                });

                var resilience = payuOptions.Resilience;

                if (resilience is { Enable: true })
                {
                    httpClientBuilder.AddResilienceHandler("Payu-transport", builder =>
                    {
                        builder
                            .AddTimeout(TimeSpan.FromSeconds(resilience.TimeoutSeconds))
                            .AddRetry(new HttpRetryStrategyOptions
                            {
                                MaxRetryAttempts = resilience.MaxRetryAttempts,
                                Delay = TimeSpan.FromSeconds(resilience.RetryBaseDelaySeconds),
                                MaxDelay = TimeSpan.FromSeconds(resilience.RetryMaxDelaySeconds),
                                BackoffType = DelayBackoffType.Exponential
                            })
                            .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                            {
                                FailureRatio = resilience.CircuitBreakerFailureRatio,
                                MinimumThroughput = resilience.CircuitBreakerMinimumThroughput,
                                BreakDuration = TimeSpan.FromSeconds(resilience.CircuitBreakerBreakDurationSeconds),
                                SamplingDuration = TimeSpan.FromSeconds(resilience.CircuitBreakerSamplingDurationSeconds)
                            });
                    });
                }
            }

        }
    }
}
