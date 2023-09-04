using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.Models.Config;
using NadekoUpdater.Services;
using NadekoUpdater.Services.Abstractions;
using NadekoUpdater.Services.Mocks;
using NadekoUpdater.Views.Windows;
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
        // Design-time
        if (Design.IsDesignMode)
        {
            serviceCollection.AddTransient<MockNadekoResolver>();
            serviceCollection.AddTransient<MockAppConfigManager>();
        }

        // Internal
        serviceCollection.AddMemoryCache();
        serviceCollection.AddSingleton<ILogWriter, LogWriter>();
        serviceCollection.AddSingleton<IBotOrchestrator, NadekoOrchestrator>();
        serviceCollection.AddSingleton(x => x.GetRequiredService<AppView>().StorageProvider);

        // Web requests
        serviceCollection.AddHttpClient();
        serviceCollection.AddHttpClient(AppConstants.NoRedirectClient)  // Client that doesn't allow automatic reditections
            .ConfigureHttpMessageHandlerBuilder(builder => builder.PrimaryHandler = new HttpClientHandler() { AllowAutoRedirect = false });

        // App settings
        serviceCollection.AddSingleton<IAppConfigManager, AppConfigManager>();
        serviceCollection.AddSingleton<ReadOnlyAppConfig>();
        serviceCollection.AddSingleton(_ =>
            (File.Exists(AppStatics.AppConfigUri))
                ? JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(AppStatics.AppConfigUri)) ?? new()
                : new()
        );

        // Dependency resolvers
        serviceCollection.AddSingleton<IYtdlpResolver, YtdlpResolver>();
        serviceCollection.AddTransient<IBotResolver, NadekoResolver>();

        // Platform-dependent services
        if (OperatingSystem.IsWindows())
            serviceCollection.AddSingleton<IFfmpegResolver, FfmpegWindowsResolver>();
        else if (OperatingSystem.IsLinux())
            serviceCollection.AddSingleton<IFfmpegResolver, FfmpegLinuxResolver>();
        else if (OperatingSystem.IsMacOS())
            serviceCollection.AddSingleton<IFfmpegResolver, FfmpegMacResolver>();
        else
            serviceCollection.AddSingleton<IFfmpegResolver, FfmpegMockResolver>();

        return serviceCollection;
    }
}
