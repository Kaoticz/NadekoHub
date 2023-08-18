using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using NadekoUpdater.Enums;
using NadekoUpdater.Services;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using NadekoUpdater.Views.Windows;
using NadekoUpdater.Services.Abstractions;
using MsBox.Avalonia.Dto;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// The view-model for the application's settings.
/// </summary>
public class ConfigViewModel : ViewModelBase<ConfigView>
{
    private readonly AppConfigManager _appConfigManager;
    private readonly AppView _mainWindow;
    private readonly IYtdlpResolver _ytdlpResolver;

    /// <summary>
    /// Contains view-models for buttons that install dependencies for Nadeko.
    /// </summary>
    public IReadOnlyList<DependencyButtonViewModel> DependencyButtons { get; }

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
    /// <param name="ffmpegResolver">The service that manages ffmpeg on the system.</param>
    /// <param name="ytdlpResolver">The service that manages yt-dlp on the system.</param>
    public ConfigViewModel(AppConfigManager appConfigManager, AppView mainWindow, UriInputBarViewModel defaultBotUriBar, IFfmpegResolver ffmpegResolver, IYtdlpResolver ytdlpResolver)
    {
        _appConfigManager = appConfigManager;
        _mainWindow = mainWindow;
        _ytdlpResolver = ytdlpResolver;

        DefaultBotUriBar = defaultBotUriBar;
        DefaultBotUriBar.CurrentUri = appConfigManager.AppConfig.BotsDirectoryUri;
        DefaultBotUriBar.OnValidUri += async (_, eventArgs) => await appConfigManager.UpdateConfigAsync(x => x.BotsDirectoryUri = eventArgs.NewUri);

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
                (string oldVer, null) => ShowDialogWindowAsync($"{dependencyResolver.DependencyName} is already up-to-date (version {oldVer})."),
                (null, string newVer) => ShowDialogWindowAsync($"{dependencyResolver.DependencyName} version {newVer} was successfully installed.", Icon.Success),
                (string oldVer, string newVer) => ShowDialogWindowAsync($"{dependencyResolver.DependencyName} was successfully updated from version {oldVer} to version {newVer}.", Icon.Success),
                (null, null) => ShowDialogWindowAsync($"Update of {dependencyResolver.DependencyName} is ongoing.", Icon.Warning, DialogType.Warning)
            };

            await dialogWindowTask;
            buttonViewModel.Status = DependencyStatus.Installed;
        }
        catch (Exception ex)
        {
            await ShowDialogWindowAsync($"An error occurred when updating {dependencyResolver.DependencyName}:\n{ex.Message}", Icon.Error, DialogType.Error);
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
        var unixNotice = (Environment.OSVersion.Platform is not PlatformID.Unix)
            ? string.Empty
            : Environment.NewLine + "To make the dependencies accessible to your bot instances without this updater, consider installing " +
            $"them through your package manager or adding the directory \"{AppStatics.AppDepsUri}\" to your PATH environment variable.";

        var dialogBox = MessageBoxManager.GetMessageBoxStandard(new()
        {
            ButtonDefinitions = ButtonEnum.Ok,
            ContentMessage = message + unixNotice,
            ContentTitle = dialogType.ToString(),
            Icon = iconType,
            MaxWidth = int.Parse(WindowConstants.DefaultWindowWidth) / 1.7,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        });

        return dialogBox.ShowWindowDialogAsync(_mainWindow);
    }
}