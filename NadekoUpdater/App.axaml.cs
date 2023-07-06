using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.Views.Windows;
using System;
using System.Reflection;

namespace NadekoUpdater;

/// <summary>
/// Defines the Avalonia application.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// IoC container with all services required by the application.
    /// </summary>
    public IServiceProvider Services { get; } = new ServiceCollection()
        .RegisterViewsAndViewModels(Assembly.GetExecutingAssembly())
        .RegisterServices()
        .BuildServiceProvider(true);

    /// <inheritdoc />
    public override void Initialize()
        => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = Services.GetRequiredService<HomeView>();

        base.OnFrameworkInitializationCompleted();
    }
}