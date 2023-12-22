﻿namespace v10.DependencyInjection.Abstractions;

/// <summary>
/// Annotate as singleton class by singleton lifetime.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute : InjectableAttribute
{
    /// <summary>
    /// Create new instance.
    /// </summary>
    public SingletonAttribute() : base(InjectionMode.Singleton)
    {
    }
}
