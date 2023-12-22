using Microsoft.Extensions.DependencyInjection;

namespace v10.Games.EightBall.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add8BallGame(this IServiceCollection services)
    {
        services.AddSingleton<IEightBallService, EightBallService>();
        return services;
    }
}
