using CodeDesignPlus.Net.Microservice.Commons.Application;
using CodeDesignPlus.Net.Microservice.Commons.EntryPoints.Rest.Middlewares;
using CodeDesignPlus.Net.Microservice.Commons.EntryPoints.Rest.Resources;
using CodeDesignPlus.Net.Microservice.Commons.EntryPoints.Rest.Swagger;
using CodeDesignPlus.Net.Microservice.Commons.FluentValidation;
using CodeDesignPlus.Net.Microservice.Commons.HealthChecks;
using CodeDesignPlus.Net.Microservice.Commons.MediatR;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Services;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;
using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu.Options;
using CodeDesignPlus.Net.Redis.Cache.Extensions;
using CodeDesignPlus.Net.Vault.Extensions;
using NodaTime.Serialization.JsonNet;

var builder = WebApplication.CreateSlimBuilder(args);

Serilog.Debugging.SelfLog.Enable(Console.Error);

builder.Host.UseSerilog();

builder.Configuration.AddVault();

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error;
        options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddVault(builder.Configuration);
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddMongo<CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Startup>(builder.Configuration);
builder.Services.AddObservability(builder.Configuration, builder.Environment);
builder.Services.AddLogger(builder.Configuration);
builder.Services.AddRabbitMQ<CodeDesignPlus.Net.Microservice.Payments.Domain.Startup>(builder.Configuration);
builder.Services.AddMapster();
builder.Services.AddFluentValidation();
builder.Services.AddMediatR<CodeDesignPlus.Net.Microservice.Payments.Application.Startup>();
builder.Services.AddSecurity(builder.Configuration);
builder.Services.AddCoreSwagger<Program>(builder.Configuration);
builder.Services.AddCache(builder.Configuration);
builder.Services.AddResources<Program>(builder.Configuration);
builder.Services.AddHealthChecksServices();

var section = builder.Configuration.GetSection(PayuOptions.Section);

builder.Services
      .AddOptions<PayuOptions>()
      .Bind(section)
      .ValidateDataAnnotations()
      .ValidateOnStart();

builder.Services.AddScoped<IPayment, Payment>();

var payuOptions = section.Get<PayuOptions>();

if (payuOptions != null && payuOptions.Enable)
{
    builder.Services.AddScoped<IPayu, Payu>();

    builder.Services.AddHttpClient("Payu", client =>
    {
        client.BaseAddress = payuOptions.Url;
    });
}

var app = builder.Build();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.UsePath();

app.UseExceptionMiddleware();
app.UseHealthChecks();
app.UseCodeErrors();

app.UseCoreSwagger();

app.UseHttpsRedirection();

app.UseAuth();

app.MapControllers().RequireAuthorization();

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}