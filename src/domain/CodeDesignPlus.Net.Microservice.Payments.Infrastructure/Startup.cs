using CodeDesignPlus.Net.Microservice.Payments.Domain.Services;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;

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

            services.AddScoped<IPayment, Payment>();

            var payuOptions = section.Get<PayuOptions>();

            if (payuOptions != null && payuOptions.Enable)
            {
                services.AddScoped<IPayu, Payu>();

                services.AddHttpClient("Payu", client =>
                {
                    client.BaseAddress = payuOptions.Url;
                });
            }

            services.AddHostedService<CodeDesignPlus.Net.Microservice.Payments.Infrastructure.BackgroundService.BankSyncBackgroundService>();


        }
    }
}
