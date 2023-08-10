using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.Views.Windows;
using System.Reflection;

namespace NadekoUpdater;

/// <summary>
/// Defines the Avalonia application.
/// </summary>
public partial class App : Application
{
    private DateTimeOffset _trayClickTime = DateTimeOffset.UnixEpoch;

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
            desktop.MainWindow = Services.GetRequiredService<AppView>();

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Executed when the "Open" menu in the tray icon is clicked.
    /// </summary>
    /// <param name="sender">A <see cref="NativeMenuItem"/>.</param>
    /// <param name="eventArgs">A <see cref="EventArgs"/>.</param>
    private void ShowApp(object sender, EventArgs eventArgs)
        => Services.GetRequiredService<AppView>().Show();

    /// <summary>
    /// Executed when the "Close" menu in the tray icon is clicked.
    /// </summary>
    /// <param name="sender">A <see cref="NativeMenuItem"/>.</param>
    /// <param name="eventArgs">A <see cref="EventArgs"/>.</param>
    private void CloseApp(object sender, EventArgs eventArgs)
        => Services.GetRequiredService<AppView>().Close();

    /// <summary>
    /// Executed when the tray icon is clicked.
    /// </summary>
    /// <param name="sender">A <see cref="TrayIcon"/>.</param>
    /// <param name="eventArgs">A <see cref="EventArgs"/>.</param>
    /// <remarks>Shows or hides the application when the tray icon is double-clicked.</remarks>
    private void TrayDoubleClick(object sender, EventArgs eventArgs)
    {
        // If this is the first click or if the second click took longer than 0.3 seconds, exit.
        if (_trayClickTime == DateTimeOffset.UnixEpoch || DateTimeOffset.Now.Subtract(_trayClickTime) > TimeSpan.FromSeconds(0.3))
        {
            _trayClickTime = DateTimeOffset.Now;
            return;
        }

        // User has double-clicked the tray icon. Reset the timer.
        _trayClickTime = DateTimeOffset.UnixEpoch;

        var mainWindow = Services.GetRequiredService<AppView>();

        if (mainWindow.IsVisible)
            mainWindow.Hide();
        else
            mainWindow.Show();
    }
}