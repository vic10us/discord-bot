using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using v10.Messaging;

namespace bot.Features.Events;

public static class MessagingExtensions {

    public static IServiceCollection AddEvents(this IServiceCollection services, IConfiguration config)
    {
        services.AddEventMessaging(config);

        services.AddHostedService<DiscordWorker>();
        return services;
    }

}
