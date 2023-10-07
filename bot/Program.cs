using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using bot.Features.DadJokes;
using bot.Features.MondayQuotes;
using bot.Features.Pictures;
using bot.Features.RedneckJokes;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using bot.Features.Games;
using bot;
using Microsoft.AspNetCore.Builder;
using bot.Services;
using MediatR;
using FluentValidation;
using bot.PipelineBehaviors;
using bot.Extensions;
using v10.Data.Abstractions;
using v10.Data.Abstractions.Interfaces;
using v10.Data.MongoDB;
using bot.Features.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using bot.Features.Metrics;
using System.Net;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Discord.Interactions;
using Discord;
using Microsoft.FeatureManagement;
using bot.Features.FeatureManagement;
using bot.Features.StrangeLaws;
using LazyProxy.ServiceProvider;
using bot.Features.NaturalLanguageProcessing;
using bot.Features.Events;
using v10.Messaging;
using v10.Games.EightBall;

Console.OutputEncoding = System.Text.Encoding.Unicode;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("hostsettings.json", optional: true);
builder.Configuration.AddEnvironmentVariables(prefix: "BOT_");
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddCommandLine(args);

// Services
var services = builder.Services;

services.AddFeatureManagement()
    .AddFeatureFilter<DevEnvironmentFilter>()
    .AddFeatureFilter<ProdTestAccountsFilter>();

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddHttpClient();
services.AddHttpClient("vic10usApi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["vic10usApi:BaseUrl"]);
});

//services.AddOpenTelemetry();

TelemetryTools.Init();
var otlpUrl = builder.Configuration["Telemetry:OtlpExporter:Url"];
var prometheusListenerUrl = builder.Configuration["Telemetry:PrometheusExporter:Url"];

// Configure metrics
services.AddOpenTelemetryMetrics(telemetryBuilder =>
{
    telemetryBuilder.AddConsoleExporter();
    telemetryBuilder.AddAspNetCoreInstrumentation();
    telemetryBuilder.AddHttpClientInstrumentation();
    telemetryBuilder.AddMeter("DiscordBotMetrics");
    var otlpUrl = builder.Configuration["Telemetry:OtlpExporter:Url"];
    telemetryBuilder.AddOtlpExporter(options => options.Endpoint = new Uri(otlpUrl));
    telemetryBuilder.AddPrometheusExporter(config =>
    {
        config.StartHttpListener = true;
        config.HttpListenerPrefixes = new[] { prometheusListenerUrl };
        TelemetryTools.Init();
    });
});

// Configure tracing
services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder.AddHttpClientInstrumentation(options =>
    {
        options.Enrich = (activity, eventName, rawObject) =>
        {
            if (eventName.Equals("OnStartActivity"))
            {
                if (rawObject is HttpWebRequest request)
                {
                    activity.SetTag("requestUri", request.RequestUri);
                }
            }
        };
    });
    tracerProviderBuilder.AddAspNetCoreInstrumentation();
    tracerProviderBuilder.AddSource("DiscordBotActivitySource");
    tracerProviderBuilder.AddConsoleExporter();
    tracerProviderBuilder.AddOtlpExporter(options => options.Endpoint = new Uri(otlpUrl));
});

// Configure logging
builder.Logging.AddOpenTelemetry(builder =>
{
    builder.IncludeFormattedMessage = true;
    builder.IncludeScopes = true;
    builder.ParseStateValues = true;
    builder.AddOtlpExporter(options => options.Endpoint = new Uri(otlpUrl));
});

services.AddHostedService<StartupBackgroundService>();
services.AddSingleton<StartupHealthCheck>();

services.AddEvents(builder.Configuration);

services.AddHealthChecks()
  .AddCheck<ImageApiHealthCheck>("ImageApi")
  .AddCheck<StartupHealthCheck>(
        "Startup",
        tags: new[] { "ready" });

//services.AddDbContext<BotDbContext>
//    (x => x.UseSqlite(telemetryBuilder.Configuration.GetConnectionString("BotDb")), ServiceLifetime.Singleton);
services.AddSingleton<DiceGame>();
services.AddSingleton(sc =>
{
    // var allExcept = GatewayIntents.All - GatewayIntents.GuildScheduledEvents;
    var config = new DiscordSocketConfig()
    {
        GatewayIntents = GatewayIntents.All ^ GatewayIntents.GuildScheduledEvents
    };
    return new DiscordSocketClient(config);
});

services.AddLazySingleton<INLPService, NLPService>();
services.AddSingleton<CommandService>();
services.AddSingleton<CommandHandlingService>();
services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
services.AddSingleton<PictureService>();
services.AddSingleton<DadJokeService>();
services.AddSingleton<EightBallService>();
services.AddTransient<Program>();
services.AddSingleton<MondayQuotesService>();
services.AddSingleton<IRedneckJokeService, RedneckJokeService>();
services.AddSingleton<IStrangeLawsService, StrangeLawsService>();
services.AddSingleton<BotDataService>();
services.AddHttpClient<DadJokeService>("DadJokeService", (s, c) =>
{
    c.BaseAddress = new Uri(builder.Configuration["DadJokes:BaseUrl"]);
});
services.AddHostedService<LifetimeEventsHostedService>();
services.AddTransient<ImageApiService>();
services.Configure<DiscordBotDatabaseSettings>(
    builder.Configuration.GetSection(nameof(DiscordBotDatabaseSettings)));

services.Configure<MassTransitOptions>(
    builder.Configuration.GetSection("MassTransit"));

services.AddSingleton<IDatabaseSettings>(sp =>
    sp.GetRequiredService<IOptions<DiscordBotDatabaseSettings>>().Value);

services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("AppCache");
    options.InstanceName = "SampleInstance";
});

//var metrics = AppMetrics.CreateDefaultBuilder()
//            .Build();
//services.AddMetrics(metrics);
services.AddMediatR(cfg =>
{
    var mediatorAssemblies = v10.Bot.Core.AssemblyScanner.GetTypesImplementingGenericInterfaces(typeof(IRequestHandler<,>), typeof(IRequestHandler<>));
    foreach (var assembly in mediatorAssemblies)
    {
        cfg.RegisterServicesFromAssemblies(assembly);
    }
});
services.AddAutoMapper(typeof(Program));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddValidatorsFromAssembly(typeof(Program).Assembly);
//services.AddLavaNode(config =>
//{
//  telemetryBuilder.Configuration?.Bind($"Victoria", config);
//});

var app = builder.Build();

// app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseFluentValidationExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

app.AddApplicationHealthChecks();

app.UseAuthorization();
//app.UseMetricsRequestTrackingMiddleware();
//app.UseMetricsAllEndpoints();
app.MapControllers();

app.Run();
