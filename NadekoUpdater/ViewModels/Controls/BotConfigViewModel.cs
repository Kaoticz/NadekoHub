using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MsBox.Avalonia.Enums;
using NadekoUpdater.Enums;
using NadekoUpdater.Services;
using NadekoUpdater.Services.Abstractions;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using NadekoUpdater.Views.Windows;
using ReactiveUI;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// View-model for <see cref="BotConfigView"/>, the window with settings and controls for a specific bot instance.
/// </summary>
public class BotConfigViewModel : ViewModelBase<BotConfigView>
{
    private Bitmap _botAvatar = LoadLocalImage();
    private string _botName = string.Empty;
    private string _directoryHint = string.Empty;
    private bool _areButtonsEnabled = true;
    private readonly AppConfigManager _appConfigManager;
    private readonly AppView _mainWindow;

    /// <summary>
    /// The bot resolver to be used.
    /// </summary>
    public IBotResolver Resolver { get; }

    /// <summary>
    /// The position of this bot instance in the lateral bot list.
    /// </summary>
    public uint Position { get; }

    /// <summary>
    /// The bar that defines where the bot instance should be saved to.
    /// </summary>
    public UriInputBarViewModel BotDirectoryUriBar { get; }

    /// <summary>
    /// The bar to update the bot instance.
    /// </summary>
    public DependencyButtonViewModel UpdateBar { get; }

    /// <summary>
    /// The bot avatar to be displayed on the front-end.
    /// </summary>
    public Bitmap BotAvatar
    {
        get => _botAvatar;
        set => this.RaiseAndSetIfChanged(ref _botAvatar, value);
    }

    /// <summary>
    /// The hint for <see cref="BotDirectoryUriBar"/>.
    /// </summary>
    public string DirectoryHint
    {
        get => _directoryHint;
        private set => this.RaiseAndSetIfChanged(ref _directoryHint, value);
    }

    /// <summary>
    /// The name of the bot.
    /// </summary>
    public string BotName
    {
        get => _botName;
        set
        {
            DirectoryHint = $"Select the absolute path to the bot directory. For example: {Path.Combine(_appConfigManager.AppConfig.BotsDirectoryUri, value)}";
            this.RaiseAndSetIfChanged(ref _botName, value);
        }
    }

    /// <summary>
    /// Determines whether if a long-running setting option is currently in progress.
    /// </summary>
    public bool IsSettingInProgress
    {
        get => _areButtonsEnabled;
        set => this.RaiseAndSetIfChanged(ref _areButtonsEnabled, value);
    }

    /// <summary>
    /// Creates a view-model for <see cref="BotConfigView"/>.
    /// </summary>
    /// <param name="appConfigManager">The app settings manager.</param>
    /// <param name="mainWindow">The main window of the application.</param>
    /// <param name="botDirectoryUriBar">The text box with the path to the directory where the bot instance is.</param>
    /// <param name="updateBotBar">The bar for updating the bot.</param>
    /// <param name="botResolver">The bot resolver to be used.</param>
    public BotConfigViewModel(AppConfigManager appConfigManager, AppView mainWindow, UriInputBarViewModel botDirectoryUriBar, DependencyButtonViewModel updateBotBar, IBotResolver botResolver)
    {
        _appConfigManager = appConfigManager;
        _mainWindow = mainWindow;
        BotDirectoryUriBar = botDirectoryUriBar;
        UpdateBar = updateBotBar;

        UpdateBar.Click += InstallOrUpdateAsync;

        var botEntry = _appConfigManager.AppConfig.BotEntries[botResolver.Position];

        Resolver = botResolver;
        BotDirectoryUriBar.CurrentUri = botEntry.InstanceDirectoryUri;
        BotAvatar = LoadLocalImage(botEntry.AvatarUri);
        BotName = botResolver.BotName;
        Position = botResolver.Position;
        UpdateBar.DependencyName = "Checking...";

        _ = LoadUpdateBarAsync(botResolver, updateBotBar);
    }

    /// <summary>
    /// Creates a backup of the bot instance associated with this view-model.
    /// </summary>
    public async ValueTask BackupBotAsync()
    {
        IsSettingInProgress = false;

        var backupUri = await Resolver.CreateBackupAsync();

        await ((string.IsNullOrWhiteSpace(backupUri))
            ? _mainWindow.ShowDialogWindowAsync($"Bot {Resolver.BotName} not found.", DialogType.Error, Icon.Error)
            : _mainWindow.ShowDialogWindowAsync($"Successfully backed up {Resolver.BotName} to:{Environment.NewLine}{backupUri}", iconType: Icon.Success));
        
        IsSettingInProgress = true;
    }

    /// <summary>
    /// Installs or updates the bot instance associated with this view-model.
    /// </summary>
    /// <param name="dependencyButton">The view-model of the dependency button that was pressed.</param>
    /// <param name="eventArgs">The event arguments.</param>
    public async Task InstallOrUpdateAsync(DependencyButtonViewModel dependencyButton, EventArgs eventArgs)
    {
        var originalStatus = UpdateBar.Status;
        UpdateBar.Status = DependencyStatus.Updating;

        try
        {
            var dialogWindowTask = await Resolver.InstallOrUpdateAsync(_appConfigManager.AppConfig.BotEntries[Resolver.Position].InstanceDirectoryUri) switch
            {
                (string oldVer, null) => _mainWindow.ShowDialogWindowAsync($"{Resolver.DependencyName} is already up-to-date (v{oldVer})."),
                (null, string newVer) => _mainWindow.ShowDialogWindowAsync($"{Resolver.DependencyName} v{newVer} was successfully installed.", iconType: Icon.Success),
                (string oldVer, string newVer) => _mainWindow.ShowDialogWindowAsync($"{Resolver.DependencyName} was successfully updated from version {oldVer} to version {newVer}.", iconType: Icon.Success),
                (null, null) => _mainWindow.ShowDialogWindowAsync($"Update of {Resolver.DependencyName} is ongoing.", DialogType.Warning, Icon.Warning)
            };

            await dialogWindowTask;
            UpdateBar.Status = DependencyStatus.Installed;
        }
        catch (Exception ex)
        {
            await _mainWindow.ShowDialogWindowAsync($"An error occurred while updating {Resolver.DependencyName}:\n{ex.Message}", DialogType.Error, Icon.Error);
            UpdateBar.Status = originalStatus;
        }
    }

    /// <summary>
    /// Loads the bot update bar.
    /// </summary>
    /// <param name="botResolver">The bot resolver.</param>
    /// <param name="updateBotBar">The update bar.</param>
    private static async Task LoadUpdateBarAsync(IBotResolver botResolver, DependencyButtonViewModel updateBotBar)
    {
        var currentVersion = await botResolver.GetCurrentVersionAsync();
        updateBotBar.DependencyName = (string.IsNullOrWhiteSpace(currentVersion))
            ? "Not Installed"
            : "NadekoBot v" + currentVersion;

        var canUpdate = await botResolver.CanUpdateAsync();
        updateBotBar.Status = (canUpdate is true)
            ? DependencyStatus.Update
            : (canUpdate is null)
                ? DependencyStatus.Install
                : DependencyStatus.Installed;
    }

    /// <summary>
    /// Loads the image at the specified location or the bot avatar placeholder one if it was not found.
    /// </summary>
    /// <param name="uri">The absolute path to the image file or <see langword="null"/> to get the avatar placeholder.</param>
    /// <returns>The requested image or the default bot avatar placeholder.</returns>
    private static Bitmap LoadLocalImage(string? uri = default)
    {
        return (File.Exists(uri))
            ? new(AssetLoader.Open(new Uri(uri)))
            : new(AssetLoader.Open(new Uri(AppStatics.BotAvatarPlaceholderUri)));
    }
}