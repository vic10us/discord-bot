using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace v10.Services.DadJokes.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDadJokes(this IServiceCollection services)
    {
        services.AddHttpClient<IDadJokeService>("DadJokeService", (s, c) => {
            var configuration = s.GetRequiredService<IConfiguration>();
            c.BaseAddress = new Uri(configuration["DadJokes:BaseUrl"]);
        });
        services.AddSingleton<IDadJokeService, DadJokeService>();
        return services;
    }
}
