namespace v10.DependencyInjection.Abstractions.ComponentModel;

/// <summary>
/// Annotate as service class by scoped lifetime.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServiceAttribute : ScopedAttribute
{
}
