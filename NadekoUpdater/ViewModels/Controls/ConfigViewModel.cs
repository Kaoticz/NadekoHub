using NadekoUpdater.Services;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// The view-model for the application's settings.
/// </summary>
public class ConfigViewModel : ViewModelBase<ConfigView>
{
    private readonly AppConfigManager _appConfigManager;

    /// <summary>
    /// Contains view-models for buttons that install dependencies for Nadeko.
    /// </summary>
    public static IReadOnlyList<DependencyButtonViewModel> DependencyButtons { get; } = new DependencyButtonViewModel[]
    {
        new() { DependencyName = "FFMPEG" },
        new() { DependencyName = "Youtube-dlp" }
    };

    /// <summary>
    /// The bar that defines where the bot instances should be saved to.
    /// </summary>
    public UriInputBarViewModel DefaultBotUriBar { get; }

    /// <summary>
    /// Determines whether the application should minimize to the system tray when closed.
    /// </summary>
    public bool MinimizeToTray
        => _appConfigManager.AppConfig.MinimizeToTray;

    /// <summary>
    /// Creates the view-model for the application's settings.
    /// </summary>
    /// <param name="defaultBotUriBar">The bar that defines where the bot instances should be saved to.</param>
    /// <param name="appConfigManager">The service that manages the application's settings.</param>
    public ConfigViewModel(UriInputBarViewModel defaultBotUriBar, AppConfigManager appConfigManager)
    {
        DefaultBotUriBar = defaultBotUriBar;
        _appConfigManager = appConfigManager;

        DefaultBotUriBar.OnValidUri += async (_, eventArgs) => await appConfigManager.UpdateConfigAsync(x => x.BotsDirectoryUri = eventArgs.NewUri);
    }

    /// <summary>
    /// Saves the minimize preference to the configuration file.
    /// </summary>
    public ValueTask ToggleMinimizeToTrayAsync()
        => _appConfigManager.UpdateConfigAsync(x => x.MinimizeToTray = !x.MinimizeToTray);
}