using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using bot.Features.DadJokes;
using bot.Features.Database;
using bot.Features.MondayQuotes;
using bot.Features.Pictures;
using bot.Features.RedneckJokes;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
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
using Victoria;
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
using bot.Features.EightBall;
using Discord;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("hostsettings.json", optional: true);
builder.Configuration.AddEnvironmentVariables(prefix: "BOT_");
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddCommandLine(args);
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("vic10usapi", c =>
{
  c.BaseAddress = new Uri(builder.Configuration["vic10usApi:BaseUrl"]);
});

//builder.Services.AddOpenTelemetry();

TelemetryTools.Init();

// Configure metrics
builder.Services.AddOpenTelemetryMetrics(builder =>
{
  builder.AddConsoleExporter();
  builder.AddAspNetCoreInstrumentation();
  builder.AddHttpClientInstrumentation();
  builder.AddMeter("DiscordBotMetrics");
  builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://10.198.2.17:4317"));
  builder.AddPrometheusExporter(config =>
  {
    config.StartHttpListener = true;
    config.HttpListenerPrefixes = new[] { "http://localhost:9464/" };
    TelemetryTools.Init();
  });
});

// Configure tracing
builder.Services.AddOpenTelemetryTracing(builder => 
{
  builder.AddHttpClientInstrumentation(options => {
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
  builder.AddAspNetCoreInstrumentation();
  builder.AddSource("DiscordBotActivitySource");
  builder.AddConsoleExporter();
  builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://10.198.2.17:4317"));
});

// Configure logging
builder.Logging.AddOpenTelemetry(builder =>
{
  builder.IncludeFormattedMessage = true;
  builder.IncludeScopes = true;
  builder.ParseStateValues = true;
  builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://10.198.2.17:4317"));
});

//using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
//            .AddMeter("HatCo.HatStore")
//            .AddPrometheusExporter(opt =>
//            {
//              opt.StartHttpListener = true;
//              opt.HttpListenerPrefixes = new string[] { $"http://localhost:9184/" };
//            })
//            .Build();

builder.Services.AddHostedService<StartupBackgroundService>();
builder.Services.AddSingleton<StartupHealthCheck>();

builder.Services.AddHealthChecks()
  .AddCheck<ImageApiHealthCheck>("ImageApi")
  .AddCheck<StartupHealthCheck>(
        "Startup",
        tags: new[] { "ready" });

builder.Services.AddDbContext<BotDbContext>
    (x => x.UseSqlite(builder.Configuration.GetConnectionString("BotDb")), ServiceLifetime.Singleton);
builder.Services.AddSingleton<DiceGame>();
builder.Services.AddSingleton(sc => {
    var config = new DiscordSocketConfig()
    {
        GatewayIntents = GatewayIntents.All
    };
    return new DiscordSocketClient(config);
});
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<CommandHandlingService>();
builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<PictureService>();
builder.Services.AddSingleton<DadJokeService>();
builder.Services.AddSingleton<EightBallService>();
builder.Services.AddTransient<Program>();
builder.Services.AddSingleton<MondayQuotesService>();
builder.Services.AddSingleton<IRedneckJokeService, RedneckJokeService>();
builder.Services.AddSingleton<BotDataService>();
builder.Services.AddHttpClient<DadJokeService>("DadJokeService", (s, c) =>
{
  c.BaseAddress = new Uri(builder.Configuration["DadJokes:BaseUrl"]);
});
builder.Services.AddHostedService<LifetimeEventsHostedService>();
builder.Services.AddTransient<ImageApiService>();
builder.Services.Configure<DiscordBotDatabaseSettings>(
    builder.Configuration.GetSection(nameof(DiscordBotDatabaseSettings)));

builder.Services.AddSingleton<IDatabaseSettings>(sp =>
    sp.GetRequiredService<IOptions<DiscordBotDatabaseSettings>>().Value);

//var metrics = AppMetrics.CreateDefaultBuilder()
//            .Build();
//builder.Services.AddMetrics(metrics);
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddLavaNode(config =>
{
  builder.Configuration?.Bind($"Victoria", config);
});

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
