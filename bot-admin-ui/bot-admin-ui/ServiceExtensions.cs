using MediatR;
using v10.Data.Abstractions;
using v10.Messaging;
using StackExchange.Redis;
using FluentValidation;
using v10.Bot.Core;
using System.Reflection;
using v10.Data.MongoDB.Extensions;

namespace BotAdminUI;

public static class ServiceExtensions
{
    public static IServiceCollection AddAdminUIServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddEventMessaging(config, () =>
        {
            var assemblies = new[] {
                Assembly.GetExecutingAssembly()!,
                Assembly.GetEntryAssembly()!,
            };
            return assemblies;
        });

        //services.AddSingleton(sc =>
        //{
        //    var c = new DiscordSocketConfig()
        //    {
        //        GatewayIntents = GatewayIntents.All ^ GatewayIntents.GuildScheduledEvents
        //    };
        //    var client = new DiscordSocketClient(c);
        //    client.LoginAsync(TokenType.Bot, config["Discord:Token"]).Wait();
        //    client.StartAsync().Wait();
        //    return client;
        //});

        //services.AddSingleton<IDiscordMessageService, DiscordMessageService>();

        services.Configure<DiscordBotDatabaseSettings>(
            config.GetSection(nameof(DiscordBotDatabaseSettings)));

        services.AddBotDataServices();

        services.Configure<MassTransitOptions>(
            config.GetSection("MassTransit"));

        services.AddSingleton(provider => new RedisConfiguration
        {
            ConnectionString = config.GetConnectionString("AppCache") ?? throw new Exception("AppCache connection string not found.")
        });

        services.AddSingleton(provider => {
            var redisConfiguration = provider.GetRequiredService<RedisConfiguration>();
            var redis = ConnectionMultiplexer.Connect(redisConfiguration.ConnectionString);
            var firstEndPoint = redis.GetEndPoints().FirstOrDefault();
            return firstEndPoint == null
                ? throw new ArgumentException("The endpoints collection was empty. Could not get an end point from Redis connection multiplexer.")
                : redis.GetServer(firstEndPoint);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = config.GetConnectionString("AppCache");
            options.InstanceName = "SampleInstance";
        });

        services.AddAutoMapper(typeof(Program));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        //services.AddMediatR(cfg =>
        //{
        //    var mediatorAssemblies = v10.Bot.Core.AssemblyScanner.GetTypesImplementingGenericInterfaces(typeof(IRequestHandler<,>), typeof(IRequestHandler<>));
        //    if (!mediatorAssemblies.Any())
        //    {
        //        cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
        //    }
        //    else
        //    {
        //        foreach (var assembly in mediatorAssemblies)
        //        {
        //            cfg.RegisterServicesFromAssemblies(assembly);
        //        }
        //    }
        //});

        return services;
    }
}
