using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace v10.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddEventMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(busConfiguration =>
        {
            busConfiguration.SetKebabCaseEndpointNameFormatter();

            busConfiguration.SetInMemorySagaRepositoryProvider();
            //busConfiguration.SetRedisSagaRepositoryProvider(config =>
            //{
            //    config.DatabaseConfiguration("");
            //});

            var entryAssembly = new[] { Assembly.GetExecutingAssembly(), Assembly.GetEntryAssembly() };
            // var executingAssembly = Assembly.GetExecutingAssembly();

            busConfiguration.AddConsumers(entryAssembly);
            busConfiguration.AddSagaStateMachines(entryAssembly);
            busConfiguration.AddSagas(entryAssembly);
            busConfiguration.AddActivities(entryAssembly);

            var massTransitOptions = new MassTransitOptions();
            configuration.GetSection("MassTransit").Bind(massTransitOptions);
            
            // elided...
            busConfiguration.UsingRabbitMq((context, rabbitMqConfiguration) =>
            {
                rabbitMqConfiguration.Host(massTransitOptions.Host, massTransitOptions.VirtualHost, hostConfig => {
                    hostConfig.Username(massTransitOptions.Username);
                    hostConfig.Password(massTransitOptions.Password);
                });
                rabbitMqConfiguration.ConfigureEndpoints(context);
            });
        });

        // services.AddHostedService<Worker>();

        return services;
    }
}
