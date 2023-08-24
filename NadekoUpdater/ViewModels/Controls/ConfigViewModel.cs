using MsBox.Avalonia.Enums;
using NadekoUpdater.Enums;
using NadekoUpdater.Services;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using NadekoUpdater.Views.Windows;
using NadekoUpdater.Services.Abstractions;
using ReactiveUI;
using Avalonia.Controls;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// The view-model for the application's settings.
/// </summary>
public class ConfigViewModel : ViewModelBase<ConfigView>
{
    private static readonly string _unixNotice = (Environment.OSVersion.Platform is not PlatformID.Unix)
        ? string.Empty
        : Environment.NewLine + "To make the dependencies accessible to your bot instances without this updater, consider installing " +
        $"them through your package manager or adding the directory \"{AppStatics.AppDepsUri}\" to your PATH environment variable.";

    private readonly AppConfigManager _appConfigManager;
    private readonly AppView _mainWindow;
    private double _maxLogSize;

    /// <summary>
    /// Defines the maximum size a log file can have, in MB.
    /// </summary>
    public double MaxLogSize
    {
        get => _maxLogSize;
        private set => this.RaiseAndSetIfChanged(ref _maxLogSize, value);
    }

    /// <summary>
    /// Contains view-models for buttons that install dependencies for Nadeko.
    /// </summary>
    public IReadOnlyList<DependencyButtonViewModel> DependencyButtons { get; }

    /// <summary>
    /// The bar that defines where the bot instances should be stored.
    /// </summary>
    public UriInputBarViewModel BotsUriBar { get; }

    /// <summary>
    /// The bar that defines where the backup of the bot instances should be stored.
    /// </summary>
    public UriInputBarViewModel BackupsUriBar { get; }

    /// <summary>
    /// The bar that defines where the backup of the bot instances should be stored.
    /// </summary>
    public UriInputBarViewModel LogsUriBar { get; }

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
    /// <param name="botsUriBar">The bar that defines where the bot instances should be stored.</param>
    /// <param name="backupsUriBar">The bar that defines where the backups of the bot instances should be stored.</param>
    /// <param name="logsUriBar">The bar that defines where the logs of the bot instances should be stored.</param>
    /// <param name="ffmpegResolver">The service that manages ffmpeg on the system.</param>
    /// <param name="ytdlpResolver">The service that manages yt-dlp on the system.</param>
    public ConfigViewModel(AppConfigManager appConfigManager, AppView mainWindow, UriInputBarViewModel botsUriBar, UriInputBarViewModel backupsUriBar,
        UriInputBarViewModel logsUriBar, IFfmpegResolver ffmpegResolver, IYtdlpResolver ytdlpResolver)
    {
        _appConfigManager = appConfigManager;
        _mainWindow = mainWindow;
        MaxLogSize = _appConfigManager.AppConfig.LogMaxSizeMb;

        BotsUriBar = botsUriBar;
        BotsUriBar.CurrentUri = appConfigManager.AppConfig.BotsDirectoryUri;
        BotsUriBar.OnValidUri += async (_, eventArgs) => await appConfigManager.UpdateConfigAsync(x => x.BotsDirectoryUri = eventArgs.NewUri);

        BackupsUriBar = backupsUriBar;
        BackupsUriBar.CurrentUri = appConfigManager.AppConfig.BotsBackupDirectoryUri;
        BackupsUriBar.OnValidUri += async (_, eventArgs) => await appConfigManager.UpdateConfigAsync(x => x.BotsBackupDirectoryUri = eventArgs.NewUri);

        LogsUriBar = logsUriBar;
        LogsUriBar.CurrentUri = appConfigManager.AppConfig.LogsDirectoryUri;
        LogsUriBar.OnValidUri += async (_, eventArgs) => await appConfigManager.UpdateConfigAsync(x => x.LogsDirectoryUri = eventArgs.NewUri);

        DependencyButtons = new DependencyButtonViewModel[]
        {
            new() { DependencyName = ffmpegResolver.DependencyName },
            new() { DependencyName = ytdlpResolver.DependencyName }
        };

        _ = InitializeDependencyButtonAsync(DependencyButtons[0], ffmpegResolver);
        _ = InitializeDependencyButtonAsync(DependencyButtons[1], ytdlpResolver);
    }

    /// <summary>
    /// Saves the minimize preference to the configuration file.
    /// </summary>
    public ValueTask ToggleMinimizeToTrayAsync()
        => _appConfigManager.UpdateConfigAsync(x => x.MinimizeToTray = !x.MinimizeToTray);

    /// <summary>
    /// Sets the value of the button spinner.
    /// </summary>
    /// <param name="spinDirection">The direction the user spun the button.</param>
    public async Task SpinMaxLogSizeAsync(SpinDirection spinDirection)
    {
        if (MaxLogSize is 0.0 && spinDirection is SpinDirection.Decrease)
            return;

        MaxLogSize = (spinDirection is SpinDirection.Increase)
            ? Math.Round(Math.Max(0.0, MaxLogSize + 0.1), 2)
            : Math.Round(Math.Max(0.0, MaxLogSize - 0.1), 2);

        await _appConfigManager.UpdateConfigAsync(x => x.LogMaxSizeMb = MaxLogSize);
    }

    /// <summary>
    /// Sets a dependency button to its appropriate state.
    /// </summary>
    /// <param name="dependencyButton">The view-model of the dependency button that needs to be initialized.</param>
    /// <param name="dependencyResolver">The dependency resolver.</param>
    private async Task InitializeDependencyButtonAsync(DependencyButtonViewModel dependencyButton, IDependencyResolver dependencyResolver)
    {
        dependencyButton.Click += async (buttonViewModel, _) => await HandleDependencyAsync(buttonViewModel, dependencyResolver);
        
        var canUpdate = await dependencyResolver.CanUpdateAsync();
        dependencyButton.Status = (canUpdate is null)
            ? DependencyStatus.Install
            : (canUpdate is true)
                ? DependencyStatus.Update
                : DependencyStatus.Installed;
    }

    /// <summary>
    /// Handles installation and update for a dependency.
    /// </summary>
    /// <param name="buttonViewModel">The dependency button that was pressed.</param>
    /// <param name="dependencyResolver">The resolver for a given dependency.</param>
    /// <exception cref="NotSupportedException">
    /// Occurs when <see cref="IDependencyResolver.InstallOrUpdateAsync(string, CancellationToken)"/> returns invalid state.
    /// </exception>
    private async ValueTask HandleDependencyAsync(DependencyButtonViewModel buttonViewModel, IDependencyResolver dependencyResolver)
    {
        var originalStatus = buttonViewModel.Status;
        buttonViewModel.Status = DependencyStatus.Updating;

        try
        {
            var dialogWindowTask = await dependencyResolver.InstallOrUpdateAsync(AppStatics.AppDepsUri) switch
            {
                (string oldVer, null) => _mainWindow.ShowDialogWindowAsync($"{dependencyResolver.DependencyName} is already up-to-date (version {oldVer})." + _unixNotice),
                (null, string newVer) => _mainWindow.ShowDialogWindowAsync($"{dependencyResolver.DependencyName} version {newVer} was successfully installed." + _unixNotice, iconType: Icon.Success),
                (string oldVer, string newVer) => _mainWindow.ShowDialogWindowAsync($"{dependencyResolver.DependencyName} was successfully updated from version {oldVer} to version {newVer}." + _unixNotice, iconType: Icon.Success),
                (null, null) => _mainWindow.ShowDialogWindowAsync($"Update of {dependencyResolver.DependencyName} is ongoing.", DialogType.Warning, Icon.Warning)
            };

            await dialogWindowTask;
            buttonViewModel.Status = DependencyStatus.Installed;
        }
        catch (Exception ex)
        {
            await _mainWindow.ShowDialogWindowAsync($"An error occurred while updating {dependencyResolver.DependencyName}:\n{ex.Message}", DialogType.Error, Icon.Error);
            buttonViewModel.Status = originalStatus;
        }
    }
}