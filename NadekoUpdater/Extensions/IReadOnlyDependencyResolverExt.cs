using Splat;
using System;

namespace NadekoUpdater.Extensions;

/// <summary>
/// Defines extension methods for <see cref="IReadonlyDependencyResolver"/>.
/// </summary>
public static class IReadOnlyDependencyResolverExt
{
    /// <summary>
    /// Gets an instance of the given <see cref="Type"/> and
    /// throws an exception if the service is not available.
    /// </summary>
    /// <param name="resolver">The resolver we are getting the service from.</param>
    /// <param name="type">The type of the service to be resolved.</param>
    /// <param name="contract">A optional value which will retrieve only a object registered with the same contract.</param>
    /// <returns>The requested <see langword="object"/>.</returns>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="resolver"/> or <paramref name="type"/> are <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Occurs when the service cannot be resolved.</exception>
    public static object GetRequiredService(this IReadonlyDependencyResolver resolver, Type type, string? contract = default)
    {
        ArgumentNullException.ThrowIfNull(nameof(resolver));
        ArgumentNullException.ThrowIfNull(nameof(type));

        var service = resolver.GetService(type, contract);

        return (service is null)
            ? throw new InvalidOperationException($"Could not resolve service of type {type.FullName}.")
            : service;
    }

    /// <summary>
    /// Gets an instance of the given <typeparamref name="T"/> and
    /// throws an exception if the service is not available.
    /// </summary>
    /// <typeparam name="T">The type for the object we want to retrieve.</typeparam>
    /// <param name="resolver">The resolver we are getting the service from.</param>
    /// <param name="contract">A optional value which will retrieve only a object registered with the same contract.</param>
    /// <returns>The requested <typeparamref name="T"/> object.</returns>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="resolver"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Occurs when <typeparamref name="T"/> cannot be resolved.</exception>
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver, string? contract = default)
    {
        ArgumentNullException.ThrowIfNull(nameof(resolver));

        var service = resolver.GetService<T>(contract);

        return (service is null)
            ? throw new InvalidOperationException($"Could not resolve service of type {typeof(T).FullName}.")
            : service;
    }
}
