using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace v10.Services.Images.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPictureServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IPictureService, PictureService>();
        return services;
    }

    public static IServiceCollection AddImagesApi(this IServiceCollection services)
    {
        services.TryAddSingleton<IImageApiService, ImageApiService>();
        return services;
    }
}
