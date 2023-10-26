using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using v10.Data.Abstractions;
using v10.Data.Abstractions.Interfaces;

namespace v10.Data.MongoDB.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBotDataServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IDatabaseSettings>(sp =>
        {
            return sp.GetRequiredService<IOptions<DiscordBotDatabaseSettings>>().Value;
        });

        services.TryAddSingleton<IBotDataService, BotDataService>();
        return services;
    }
}
