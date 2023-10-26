using Microsoft.Extensions.DependencyInjection;

namespace v10.Games.Dice.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiceGame(this IServiceCollection services)
    {
        services.AddSingleton<IDiceGameService, DiceGameService>();
        return services;
    }
}
