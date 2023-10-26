using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace v10.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddEventMessaging(this IServiceCollection services, IConfiguration configuration, Func<IEnumerable<Assembly>> configureAssemblies = null)
    {
        services.AddMassTransit(busConfiguration =>
        {
            busConfiguration.SetKebabCaseEndpointNameFormatter();

            busConfiguration.SetInMemorySagaRepositoryProvider();
            //busConfiguration.SetRedisSagaRepositoryProvider(config =>
            //{
            //    config.DatabaseConfiguration("");
            //});

            var entryAssemblies = configureAssemblies != null ? configureAssemblies.Invoke().ToArray() : new[] { Assembly.GetExecutingAssembly() };

            // var entryAssemblies = new[] { Assembly.GetExecutingAssembly(), Assembly.GetEntryAssembly() };
            // var executingAssembly = Assembly.GetExecutingAssembly();

            busConfiguration.AddConsumers(entryAssemblies);
            busConfiguration.AddSagaStateMachines(entryAssemblies);
            busConfiguration.AddSagas(entryAssemblies);
            busConfiguration.AddActivities(entryAssemblies);

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

        return services;
    }
}
