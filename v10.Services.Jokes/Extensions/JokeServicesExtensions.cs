using Microsoft.Extensions.Configuration;
using Scrutor;
using System.Reflection;
using v10.Services.Jokes;
using v10.Options.Microsoft;

namespace Microsoft.Extensions.DependencyInjection;

public static class JokeServicesExtensions
{
    public static IServiceCollection AddJokes(this IServiceCollection services, IConfiguration config)
    {
        // var assemblies = new List<Assembly>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //foreach (string assemblyPath in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories))
        //{
        //    var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        //    assemblies.Add(assembly);
        //}
        services.AddJokes(s => s.FromAssemblies(assemblies));
        services.ConfigureOptionsRegistries(config, assemblies.ToArray());
        return services;
    }

    public static IServiceCollection AddJokes(this IServiceCollection services, params Func<IAssemblySelector, IImplementationTypeSelector>[] assemblySelectors)
    {
        foreach (var selector in assemblySelectors)
        {
            services.BuildServiceProvider();
            services.Scan(scan => selector(scan)
                .AddClasses(c => c.AssignableTo<IJokeServiceConfiguration>())
                .AsSelf()
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.Scan(scan => selector(scan)
                .AddClasses(c => c.AssignableTo<IJokeServiceImpl>())
                .AsSelf()
                .AsImplementedInterfaces()
                .WithSingletonLifetime());
        }

        return services;
    }
}
