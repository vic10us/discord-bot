using Microsoft.Extensions.DependencyInjection;

namespace v10.Services.RedneckJokes.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedneckJokes(this IServiceCollection services)
    {
        services.AddSingleton<IRedneckJokeService, RedneckJokeService>();
        return services;
    }
}
