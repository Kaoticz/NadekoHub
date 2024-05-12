using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NadekoHub.Features.AppConfig.Models;
using NadekoHub.Features.AppConfig.Services;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using NadekoHub.Features.AppConfig.Services.Mocks;
using NadekoHub.Features.AppWindow.Views.Windows;
using NadekoHub.Features.BotConfig.Services;
using NadekoHub.Features.BotConfig.Services.Abstractions;
using NadekoHub.Features.BotConfig.Services.Mocks;
using NadekoHub.Features.Home.Services;
using NadekoHub.Features.Home.Services.Abstractions;
using NadekoHub.Services;
using ReactiveUI;
using System.Reflection;
using System.Text.Json;

namespace NadekoHub.Extensions;

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
        serviceCollection.AddSingleton<IAppResolver, AppResolver>();
        serviceCollection.AddSingleton<IBotOrchestrator, NadekoOrchestrator>();
        serviceCollection.AddSingleton(x => x.GetRequiredService<AppView>().StorageProvider);

        // Web requests
        serviceCollection.AddHttpClient();
        serviceCollection.AddHttpClient(AppConstants.NoRedirectClient)  // Client that doesn't allow automatic reditections
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { AllowAutoRedirect = false });
        serviceCollection.AddHttpClient(AppConstants.GithubClient)
            .ConfigureHttpClient(x =>
            {
                x.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
                x.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            #if DEBUG
                x.DefaultRequestHeaders.UserAgent.TryParseAdd($"NadekoHub v{AppStatics.AppVersion}-Debug");
            #else
                x.DefaultRequestHeaders.UserAgent.TryParseAdd($"NadekoHub v{AppStatics.AppVersion}");
            #endif
            });

        // App settings
        serviceCollection.AddSingleton<IAppConfigManager, AppConfigManager>();
        serviceCollection.AddSingleton<ReadOnlyAppSettings>();
        serviceCollection.AddSingleton(_ =>
            (File.Exists(AppStatics.AppConfigUri))
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(AppStatics.AppConfigUri)) ?? new()
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