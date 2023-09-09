using Microsoft.Extensions.DependencyInjection;

namespace NadekoHub.Extensions;

/// <summary>
/// Defines extension methods for <see cref="IServiceProvider"/>.
/// </summary>
public static class IServiceProviderExt
{
    /// <summary>
    /// Gets service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
    /// <param name="arguments"></param>
    /// <remarks>Do not use abstract types in the type argument!</remarks>
    /// <returns>A service object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="serviceProvider"/> or <paramref name="arguments"/> are <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Occurs when there is no concrete service of type <typeparamref name="T"/> or when the arguments are wrong.</exception>
    public static T GetParameterizedService<T>(this IServiceProvider serviceProvider, params object[] arguments)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        var result = ActivatorUtilities.CreateInstance<T>(serviceProvider, arguments);

        return (result is null)
            ? throw new InvalidOperationException($"There is no service of type {nameof(T)} or the arguments were incorrect.")
            : result;
    }
}