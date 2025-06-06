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

            services.AddSingleton<IPayment, Payment>();

            var payuOptions = section.Get<PayuOptions>();

            if (payuOptions != null && payuOptions.Enabled)
            {
                services.AddSingleton<IPayu, Payu>();

                services.AddHttpClient("Payu", client =>
                {
                    client.BaseAddress = new Uri(payuOptions.BaseUrl);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });
            }
        }
    }
}
