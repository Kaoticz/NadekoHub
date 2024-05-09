using Avalonia.Controls;
using Avalonia.Styling;
using MsBox.Avalonia.Enums;
using NadekoHub.Enums;
using NadekoHub.Features.Abstractions;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using NadekoHub.Features.AppConfig.Views.Controls;
using NadekoHub.Features.AppConfig.Views.Windows;
using NadekoHub.Features.AppWindow.Views.Windows;
using NadekoHub.Features.Shared.Services.Abstractions;
using NadekoHub.Features.Shared.ViewModels;
using ReactiveUI;
using System.Diagnostics;

namespace NadekoHub.Features.AppConfig.ViewModels;

/// <summary>
/// The view-model for the application's settings.
/// </summary>
public class ConfigViewModel : ViewModelBase<ConfigView>
{
    private static readonly string _unixNotice = Environment.OSVersion.Platform is not PlatformID.Unix
        ? string.Empty
        : Environment.NewLine + "To make the dependencies accessible to your bot instances without this updater, consider installing " +
        $"them through your package manager or adding the directory \"{AppStatics.AppDepsUri}\" to your PATH environment variable.";

    private readonly IAppConfigManager _appConfigManager;
    private readonly AboutMeViewModel _aboutMeViewModel;
    private readonly AppView _mainWindow;
    private double _maxLogSize;
    private int _selectedThemeIndex;

    /// <summary>
    /// Defines the maximum size a log file can have, in MB.
    /// </summary>
    public double MaxLogSize
    {
        get => _maxLogSize;
        private set => this.RaiseAndSetIfChanged(ref _maxLogSize, value);
    }

    /// <summary>
    /// Defines the index of the theme currently selected.
    /// </summary>
    public int SelectedThemeIndex
    {
        get => _selectedThemeIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedThemeIndex, value);
            _ = ChangeThemeAsync((ThemeType)value);
        }
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
    /// <param name="aboutMeViewModel">The view-model for the AboutMe window.</param>
    /// <param name="ffmpegResolver">The service that manages ffmpeg on the system.</param>
    /// <param name="ytdlpResolver">The service that manages yt-dlp on the system.</param>
    public ConfigViewModel(IAppConfigManager appConfigManager, AppView mainWindow, UriInputBarViewModel botsUriBar, UriInputBarViewModel backupsUriBar,
        UriInputBarViewModel logsUriBar, AboutMeViewModel aboutMeViewModel, IFfmpegResolver ffmpegResolver, IYtdlpResolver ytdlpResolver)
    {
        _appConfigManager = appConfigManager;
        _mainWindow = mainWindow;
        _maxLogSize = _appConfigManager.AppConfig.LogMaxSizeMb;
        _selectedThemeIndex = (int)_appConfigManager.AppConfig.Theme;
        _aboutMeViewModel = aboutMeViewModel;

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
            new(mainWindow) { DependencyName = ffmpegResolver.DependencyName },
            new(mainWindow) { DependencyName = ytdlpResolver.DependencyName }
        };

        _ = InitializeDependencyButtonAsync(DependencyButtons[0], ffmpegResolver);
        _ = InitializeDependencyButtonAsync(DependencyButtons[1], ytdlpResolver);
    }

    /// <summary>
    /// Shows the "About Me" window as a dialog window.
    /// </summary>
    public async ValueTask OpenAboutMeAsync()
    {
        // This looks weird, but AboutMeView is rendered useless once it is closed by ShowDialog.
        var aboutMeView = new AboutMeView()
        {
            RequestedThemeVariant = _mainWindow.ActualThemeVariant,
            ViewModel = _aboutMeViewModel
        };

        await aboutMeView.ShowDialog(_mainWindow);
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

        MaxLogSize = spinDirection is SpinDirection.Increase
            ? Math.Round(Math.Max(0.0, MaxLogSize + 0.1), 2)
            : Math.Round(Math.Max(0.0, MaxLogSize - 0.1), 2);

        await _appConfigManager.UpdateConfigAsync(x => x.LogMaxSizeMb = MaxLogSize);
    }

    /// <summary>
    /// Changes the current theme to the specified theme.
    /// </summary>
    /// <param name="selectedTheme">The theme to be applied.</param>
    /// <exception cref="UnreachableException">Occurs when <see cref="ThemeType"/> has an unimplemented value.</exception>
    private async Task ChangeThemeAsync(ThemeType selectedTheme)
    {
        try
        {
            // Set the window theme
            _mainWindow.RequestedThemeVariant = selectedTheme switch
            {
                ThemeType.Auto => ThemeVariant.Default,
                ThemeType.Light => ThemeVariant.Light,
                ThemeType.Dark => ThemeVariant.Dark,
                _ => throw new UnreachableException($"No implementation for theme of type {selectedTheme} was provided."),
            };

            // Update the color of the dependency buttons
            foreach (var dependencyButton in DependencyButtons)
                dependencyButton.RecheckButtonColor();

            // Update the application settings
            await _appConfigManager.UpdateConfigAsync(x => x.Theme = selectedTheme);
        }
        catch (Exception ex)
        {
            await _mainWindow.ShowDialogWindowAsync("An error occurred when setting a theme:\n" + ex.Message, DialogType.Error, Icon.Error);
        }
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
        dependencyButton.Status = canUpdate switch
        {
            true => DependencyStatus.Update,
            false => DependencyStatus.Installed,
            null => DependencyStatus.Install
        };
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