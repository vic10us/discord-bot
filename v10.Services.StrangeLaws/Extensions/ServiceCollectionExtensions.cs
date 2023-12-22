using Microsoft.Extensions.DependencyInjection;

namespace v10.Services.StrangeLaws.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStrangeLaws(this IServiceCollection services)
    {
        services.AddSingleton<IStrangeLawsService, StrangeLawsService>();
        return services;
    }
}
