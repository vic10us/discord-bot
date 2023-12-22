using MediatR;
using v10.Data.Abstractions;
using v10.Messaging;
using StackExchange.Redis;
using FluentValidation;
using v10.Bot.Core;
using Discord.WebSocket;
using Discord;
using v10.Bot.Discord;
using System.Reflection;
using v10.Data.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configuration
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("hostsettings.json", optional: true);
builder.Configuration.AddEnvironmentVariables(prefix: "BOT_");
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddCommandLine(args);

var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddEventMessaging(builder.Configuration, () =>
{
    var assemblies = new[] {
                Assembly.GetExecutingAssembly()!,
                Assembly.GetEntryAssembly()!,
            };
    return assemblies;
});

services.AddSingleton(sc =>
{
    var config = new DiscordSocketConfig()
    {
        GatewayIntents = GatewayIntents.All ^ GatewayIntents.GuildScheduledEvents
    };
    return new DiscordSocketClient(config);
});

services.AddSingleton<IDiscordMessageService, DiscordMessageService>();

services.Configure<DiscordBotDatabaseSettings>(
    builder.Configuration.GetSection(nameof(DiscordBotDatabaseSettings)));

services.AddBotDataServices();

services.Configure<MassTransitOptions>(
    builder.Configuration.GetSection("MassTransit"));

builder.Services.AddSingleton(provider => new RedisConfiguration
{
    ConnectionString = builder.Configuration.GetConnectionString("AppCache") ?? throw new Exception("AppCache connection string not found.")
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

services.AddAutoMapper(typeof(Program));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddValidatorsFromAssembly(typeof(Program).Assembly);

services.AddMediatR(cfg =>
{
    var mediatorAssemblies = v10.Bot.Core.AssemblyScanner.GetTypesImplementingGenericInterfaces(typeof(IRequestHandler<,>), typeof(IRequestHandler<>));
    if (!mediatorAssemblies.Any())
    {
        cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
    }
    else
    {
        foreach (var assembly in mediatorAssemblies)
        {
            cfg.RegisterServicesFromAssemblies(assembly);
        }
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
