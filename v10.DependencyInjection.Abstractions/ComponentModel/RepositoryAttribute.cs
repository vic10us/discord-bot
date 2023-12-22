namespace v10.DependencyInjection.Abstractions.ComponentModel;

/// <summary>
/// Annotate as repository class by scoped lifetime.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RepositoryAttribute : ScopedAttribute
{
}
