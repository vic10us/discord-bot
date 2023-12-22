using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using bot;
using Microsoft.AspNetCore.Builder;
using MediatR;
using FluentValidation;
using bot.Extensions;
using v10.Data.Abstractions;
using bot.Features.HealthChecks;
using Discord.Interactions;
using Discord;
using Microsoft.FeatureManagement;
using bot.Features.FeatureManagement;
using LazyProxy.ServiceProvider;
using bot.Features.NaturalLanguageProcessing;
using v10.Messaging;
using v10.Bot.Discord;
using StackExchange.Redis;
using System.Linq;
using v10.Bot.Core;
using v10.Events.Core;
using System.Reflection;
using v10.Services.DadJokes.Extensions;
using v10.Services.StrangeLaws.Extensions;
using v10.Services.RedneckJokes.Extensions;
using v10.Services.MondayQuotes.Extensions;
using v10.Games.Dice.Extensions;
using v10.Games.EightBall.Extensions;
using v10.Services.Images.Extensions;
using v10.Data.MongoDB.Extensions;

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

services.AddHostedService<StartupBackgroundService>();
services.AddSingleton<StartupHealthCheck>();

services.AddEventMessaging(builder.Configuration, () =>
{
    var assemblies = new[] {
                Assembly.GetExecutingAssembly()!,
                Assembly.GetEntryAssembly()!,
                typeof(v10.Messaging.Consumers.HelloMessageConsumer).Assembly,
                typeof(v10.Events.Core.MessageBus.Consumers.UpdateAllServerStatsCommandConsumer).Assembly,
            };
    return assemblies;
});

services.Configure<DiscordWorkerOptions>(builder.Configuration.GetSection("DiscordWorker"));
services.AddHostedService<DiscordWorker>();

services.AddHealthChecks()
  .AddCheck<ImageApiHealthCheck>("ImageApi")
  .AddCheck<StartupHealthCheck>("Startup", tags: new[] { "ready" });

services.AddSingleton(sc =>
{
    var config = new DiscordSocketConfig()
    {
        GatewayIntents = GatewayIntents.All ^ GatewayIntents.GuildScheduledEvents
    };
    return new DiscordSocketClient(config);
});

services.AddSingleton<IDiscordMessageService, DiscordMessageService>();
services.AddLazySingleton<INLPService, NLPService>();
services.AddSingleton<CommandService>();
services.AddSingleton<CommandHandlingService>();
services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));

services.AddDiceGame();

services.AddTransient<Program>();

services.AddImagesApi();
services.AddPictureServices();
services.Add8BallGame();

services.AddMondayQuotes();
services.AddRedneckJokes();
services.AddStrangeLaws();
services.AddDadJokes();

services.AddHostedService<LifetimeEventsHostedService>();

services.Configure<DiscordBotDatabaseSettings>(
    builder.Configuration.GetSection(nameof(DiscordBotDatabaseSettings)));

services.AddBotDataServices();

services.Configure<MassTransitOptions>(
    builder.Configuration.GetSection("MassTransit"));

builder.Services.AddSingleton(provider => new RedisConfiguration
{
    ConnectionString = builder.Configuration.GetConnectionString("AppCache")
});

builder.Services.AddSingleton(provider => { 
    var redisConfiguration = provider.GetRequiredService<RedisConfiguration>();
    var redis = ConnectionMultiplexer.Connect(redisConfiguration.ConnectionString);
    var firstEndPoint = redis.GetEndPoints().FirstOrDefault();
    return firstEndPoint == null
        ? throw new ArgumentException("The endpoints collection was empty. Could not get an end point from Redis connection multiplexer.")
        : redis.GetServer(firstEndPoint);
});

services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("AppCache");
    options.InstanceName = "SampleInstance";
});

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

var app = builder.Build();

app.UseFluentValidationExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

app.AddApplicationHealthChecks();

app.UseAuthorization();
app.MapControllers();

app.Run();
