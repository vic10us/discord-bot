using System.Reflection;

namespace v10.Bot.Core;

public static class AssemblyScanner
{
    public static IEnumerable<Type> GetTypesImplementingInterface(Type interfaceType)
    {
        // Get all loaded assemblies in the current application domain
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Use LINQ to find types that implement the given interface
        var implementingTypes = from assembly in assemblies
                                from type in assembly.GetTypes()
                                where interfaceType.IsAssignableFrom(type) && !type.IsInterface
                                select type;

        return implementingTypes;
    }

    public static IEnumerable<Assembly> GetUniqueAssembliesImplementingInterface(Type interfaceType)
    {
        // Get all loaded assemblies in the current application domain
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Use LINQ to find unique assemblies that contain types implementing the given interface
        var uniqueAssemblies = assemblies
            .SelectMany(assembly => assembly.GetTypes()
                .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface)
                .Select(type => assembly))
            .Distinct();

        return uniqueAssemblies;
    }

    public static IEnumerable<Assembly> GetGenericInterfaces(Type genericInterfaceType)
    {
        // Get all loaded assemblies in the current application domain
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Use LINQ to find types that implement the generic interface
        var implementingTypes = from assembly in assemblies
                                from type in assembly.GetTypes()
                                where type.GetInterfaces().Any(interfaceType =>
                                    interfaceType.IsGenericType &&
                                    interfaceType.GetGenericTypeDefinition() == genericInterfaceType)
                                select type.Assembly;

        return implementingTypes.Distinct();
    }

    public static IEnumerable<Assembly> GetTypesImplementingGenericInterfaces(params Type[] genericInterfaceTypes)
    {
        // Get all loaded assemblies in the current application domain
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Use LINQ to find types that implement any of the specified generic interfaces
        var implementingTypes = from assembly in assemblies
                                from type in assembly.GetTypes()
                                where genericInterfaceTypes.Any(interfaceType =>
                                    type.GetInterfaces().Any(it =>
                                        (it.IsGenericType && it.GetGenericTypeDefinition() == interfaceType) ||
                                        (!it.IsInterface && it.IsAssignableFrom(interfaceType))
                                    ))
                                select type.Assembly;

        return implementingTypes.Distinct();
    }
}
