using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using NadekoUpdater.Enums;
using NadekoUpdater.Services;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using NadekoUpdater.Views.Windows;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// The view-model for the application's settings.
/// </summary>
public class ConfigViewModel : ViewModelBase<ConfigView>
{
    private readonly AppConfigManager _appConfigManager;
    private readonly AppView _mainWindow;
    private readonly YtdlpResolver _ytdlpResolver;

    /// <summary>
    /// Contains view-models for buttons that install dependencies for Nadeko.
    /// </summary>
    public IReadOnlyList<DependencyButtonViewModel> DependencyButtons { get; } = new DependencyButtonViewModel[]
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
    /// <param name="appConfigManager">The service that manages the application's settings.</param>
    /// <param name="mainWindow">The main window of the application.</param>
    /// <param name="defaultBotUriBar">The bar that defines where the bot instances should be saved to.</param>
    /// <param name="ytdlpResolver">The service that manages yt-dlp on the system.</param>
    public ConfigViewModel(AppConfigManager appConfigManager, AppView mainWindow, UriInputBarViewModel defaultBotUriBar, YtdlpResolver ytdlpResolver)
    {
        _appConfigManager = appConfigManager;
        _mainWindow = mainWindow;
        _ytdlpResolver = ytdlpResolver;

        DefaultBotUriBar = defaultBotUriBar;
        DefaultBotUriBar.CurrentUri = appConfigManager.AppConfig.BotsDirectoryUri;
        DefaultBotUriBar.OnValidUri += async (_, eventArgs) => await appConfigManager.UpdateConfigAsync(x => x.BotsDirectoryUri = eventArgs.NewUri);

        _ = UpdateDependencyButtonsAsync(ytdlpResolver);
    }

    /// <summary>
    /// Saves the minimize preference to the configuration file.
    /// </summary>
    public ValueTask ToggleMinimizeToTrayAsync()
        => _appConfigManager.UpdateConfigAsync(x => x.MinimizeToTray = !x.MinimizeToTray);

    /// <summary>
    /// Sets the dependency buttons to their correct state.
    /// </summary>
    /// <param name="ytdlpResolver">The yt-dlp resolver.</param>
    private async Task UpdateDependencyButtonsAsync(YtdlpResolver ytdlpResolver)
    {
        DependencyButtons[1].Click += HandleYtdlpAsync;

        // ffmpeg
        // TODO: ffmpeg

        // yt-dlp
        var ytdlpUpdate = await ytdlpResolver.CanUpdateAsync();

        DependencyButtons[1].Status = (ytdlpUpdate is null)
            ? DependencyStatus.Install
            : (ytdlpUpdate is true)
                ? DependencyStatus.Update
                : DependencyStatus.Installed;
    }

    /// <summary>
    /// Handles installation and update for yt-dlp.
    /// </summary>
    /// <param name="buttonViewModel">The dependency button that was pressed.</param>
    /// <param name="eventArgs">The event arguments.</param>
    /// <exception cref="NotSupportedException">
    /// Occurs when <see cref="YtdlpResolver.InstallOrUpdateYtdlpAsync(string)"/> returns invalid state.
    /// </exception>
    private async Task HandleYtdlpAsync(DependencyButtonViewModel buttonViewModel, EventArgs eventArgs)
    {
        var originalStatus = buttonViewModel.Status;
        buttonViewModel.Status = DependencyStatus.Updating;

        var dialogWindowTask = await _ytdlpResolver.InstallOrUpdateYtdlpAsync(AppStatics.AppDepsUri) switch
        {
            (string oldVer, null) => ShowDialogWindowAsync($"Yt-dlp is already up-to-date (version {oldVer})."),
            (null, string newVer) => ShowDialogWindowAsync($"Yt-dlp version {newVer} was successfully installed.", Icon.Success),
            (string oldVer, string newVer) => ShowDialogWindowAsync($"Yt-dlp was successfully updated from version {oldVer} to version {newVer}.", Icon.Success),
            _ => throw new NotSupportedException($"{nameof(YtdlpResolver.InstallOrUpdateYtdlpAsync)} returned invalid state.")
        };

        try
        {
            await dialogWindowTask;
            buttonViewModel.Status = DependencyStatus.Installed;
        }
        catch (Exception ex)
        {
            await ShowDialogWindowAsync("An error occurred when updating yt-dlp:\n" + ex.Message, Icon.Error, DialogType.Error);
            buttonViewModel.Status = originalStatus;
        }
    }

    /// <summary>
    /// Shows a dialog window that blocks the main window.
    /// </summary>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="iconType">The icon to be displayed.</param>
    /// <param name="dialogType">The type of dialog window to display.</param>
    private Task ShowDialogWindowAsync(string message, Icon iconType = Icon.None, DialogType dialogType = DialogType.Notification)
    {
        var dialogBox = MessageBoxManager.GetMessageBoxStandard(dialogType.ToString(), message, ButtonEnum.Ok, iconType, WindowStartupLocation.CenterOwner);
        return dialogBox.ShowWindowDialogAsync(_mainWindow);
    }
}