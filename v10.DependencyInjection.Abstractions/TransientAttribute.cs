﻿namespace v10.DependencyInjection.Abstractions;

/// <summary>
/// Annotate as transient class by transient lifetime.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TransientAttribute : InjectableAttribute
{
    /// <summary>
    /// Create new instance.
    /// </summary>
    public TransientAttribute() : base(InjectionMode.Transient)
    {
    }
}
