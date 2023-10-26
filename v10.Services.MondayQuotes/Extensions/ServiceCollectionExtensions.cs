using Microsoft.Extensions.DependencyInjection;

namespace v10.Services.MondayQuotes.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMondayQuotes(this IServiceCollection services)
    {
        services.AddSingleton<IMondayQuotesService, MondayQuotesService>();
        return services;
    }
}
