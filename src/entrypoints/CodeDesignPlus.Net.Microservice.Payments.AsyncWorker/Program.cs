using CodeDesignPlus.Net.gRpc.Clients.Extensions;
using CodeDesignPlus.Net.Hangfire.Extensions;
using CodeDesignPlus.Net.Logger.Extensions;
using CodeDesignPlus.Net.Microservice.Commons.FluentValidation;
using CodeDesignPlus.Net.Microservice.Commons.HealthChecks;
using CodeDesignPlus.Net.Microservice.Commons.MediatR;
using CodeDesignPlus.Net.Mongo.Extensions;
using CodeDesignPlus.Net.Observability.Extensions;
using CodeDesignPlus.Net.RabbitMQ.Extensions;
using CodeDesignPlus.Net.ServiceBus.Extensions;
using CodeDesignPlus.Net.Redis.Cache.Extensions;
using CodeDesignPlus.Net.Redis.Extensions;
using CodeDesignPlus.Net.Security.Extensions;
using CodeDesignPlus.Net.Vault.Extensions;
using Mapster;

var builder = WebApplication.CreateSlimBuilder(args);

Serilog.Debugging.SelfLog.Enable(Console.Error);

builder.Host.UseSerilog();

builder.Configuration.AddVault();

builder.Services.AddVault(builder.Configuration);
builder.Services.AddMongo<CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Startup>(builder.Configuration);
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddRabbitMQ<Program>(builder.Configuration);
builder.Services.AddServiceBus<Program>(builder.Configuration);
builder.Services.AddSecurity(builder.Configuration);
builder.Services.AddCache(builder.Configuration);
builder.Services.AddMapster();
builder.Services.AddFluentValidation();
builder.Services.AddMediatR<CodeDesignPlus.Net.Microservice.Payments.Application.Startup>();
builder.Services.AddHealthChecksServices();
builder.Services.AddObservability(builder.Configuration, builder.Environment);
builder.Services.AddGrpcClients(builder.Configuration);

// El barrido que cierra los cobros que se quedaron en curso. Es el primer trabajo programado de este
// servicio: hasta ahora solo tenia servicios en segundo plano, que corren en cada replica y no se
// coordinan entre si.
builder.Services.AddHangfire<Program>(builder.Configuration);
builder.Services.AddHostedService<CodeDesignPlus.Net.Microservice.Payments.Infrastructure.BackgroundService.BankSyncBackgroundService>();
builder.Services.AddHostedService<CodeDesignPlus.Net.Microservice.Payments.Infrastructure.BackgroundService.PaymentMethodSeedBackgroundService>();

var app = builder.Build();

app.UseHealthChecks();
app.UseHangfireDashboard<Program>(builder.Configuration);
    
var home = app.MapGroup("/");

home.MapGet("/", () => "Ready");

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}