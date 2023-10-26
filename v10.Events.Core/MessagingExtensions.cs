using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using v10.Messaging;

namespace v10.Events.Core;

public static class MessagingExtensions
{
    public static IServiceCollection AddEvents(this IServiceCollection services, IConfiguration config)
    {
        _ = services.AddEventMessaging(config, () =>
        {
            var assemblies = new[] { 
                Assembly.GetExecutingAssembly()!, 
                Assembly.GetEntryAssembly()! 
            };
            return assemblies;
        });

        services.AddHostedService<DiscordWorker>();
        return services;
    }
}
