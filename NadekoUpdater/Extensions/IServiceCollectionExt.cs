using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.Models.Config;
using NadekoUpdater.Services;
using ReactiveUI;
using System.Reflection;
using System.Text.Json;

namespace NadekoUpdater.Extensions;

/// <summary>
/// Defines extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class IServiceCollectionExt
{
    /// <summary>
    /// Http handler to stop redirections from happening.
    /// </summary>
    private static readonly HttpClientHandler _httpHandler = new() { AllowAutoRedirect = false };

    /// <summary>
    /// Registers all views and view-models in the provided <paramref name="assembly"/> to this service collection.
    /// </summary>
    /// <param name="serviceCollection">This service collection.</param>
    /// <param name="assembly">The assembly to get the views and view-models from.</param>
    /// <returns>This service collection with the views and view-models added.</returns>
    public static IServiceCollection RegisterViewsAndViewModels(this IServiceCollection serviceCollection, Assembly assembly)
    {
        var viewModelPairs = assembly.GetTypes()
            .Where(x => !x.IsAbstract && x.IsAssignableTo(typeof(IViewFor)))
            .Select(x => (ViewType: x, ViewModelType: x.GetInterface(typeof(IViewFor<>).Name)!.GenericTypeArguments[0]));

        foreach (var (viewType, viewModelType) in viewModelPairs)
        {
            serviceCollection.AddSingleton(viewType);
            serviceCollection.AddTransient(viewModelType);
        }

        return serviceCollection;
    }

    /// <summary>
    /// Registers the application's services.
    /// </summary>
    /// <param name="serviceCollection">This service collection.</param>
    /// <returns>This service collection with the services added.</returns>
    public static IServiceCollection RegisterServices(this IServiceCollection serviceCollection)
    {
        // Web requests
        serviceCollection.AddSingleton<YtdlpResolver>();
        serviceCollection.AddHttpClient();
        serviceCollection.AddHttpClient(AppStatics.NoRedirectClient)
            .ConfigureHttpMessageHandlerBuilder(builder => builder.PrimaryHandler = _httpHandler);

        // App settings
        serviceCollection.AddSingleton<AppConfigManager>();
        serviceCollection.AddSingleton<ReadOnlyAppConfig>();
        serviceCollection.AddSingleton(_ =>
            (File.Exists(AppStatics.AppConfigUri))
                ? JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(AppStatics.AppConfigUri)) ?? new()
                : new()
        );

        return serviceCollection;
    }
}
