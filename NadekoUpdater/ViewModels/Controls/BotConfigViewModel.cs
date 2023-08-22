using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Kotz.Events;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NadekoUpdater.Enums;
using NadekoUpdater.Services;
using NadekoUpdater.Services.Abstractions;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using NadekoUpdater.Views.Windows;
using ReactiveUI;
using System.Diagnostics;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// View-model for <see cref="BotConfigView"/>, the window with settings and controls for a specific bot instance.
/// </summary>
public class BotConfigViewModel : ViewModelBase<BotConfigView>
{
    private Bitmap _botAvatar = LoadLocalImage();
    private string _botName = string.Empty;
    private string _directoryHint = string.Empty;
    private bool _areButtonsUnlocked;
    private bool _isIdle;
    private readonly AppConfigManager _appConfigManager;
    private readonly AppView _mainWindow;

    /// <summary>
    /// Triggered when the user deletes the bot instance associated with this view-model.
    /// </summary>
    public event AsyncEventHandler<BotConfigViewModel, EventArgs>? BotDeleted;

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
    public bool AreButtonsUnlocked
    {
        get => _areButtonsUnlocked;
        private set => this.RaiseAndSetIfChanged(ref _areButtonsUnlocked, value);
    }

    /// <summary>
    /// Determines whether this view-model is undergoing an operation or not.
    /// </summary>
    public bool IsIdle
    {
        get => _isIdle;
        private set => this.RaiseAndSetIfChanged(ref _isIdle, value);
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

        EnableButtons(!Directory.Exists(botEntry.InstanceDirectoryUri), true);
        _ = LoadUpdateBarAsync(botResolver, updateBotBar);
    }
 
    /// <summary>
    /// Moves or renames the bot instance associated with this view-model.
    /// </summary>
    public async ValueTask MoveOrRenameAsync()
    {
        var wereButtonsUnlocked = AreButtonsUnlocked;
        EnableButtons(true, false);

        var botEntry = _appConfigManager.AppConfig.BotEntries[Resolver.Position];
        var oldName = botEntry.Name;
        var oldUri = botEntry.InstanceDirectoryUri;
        var hasNewUri = !BotDirectoryUriBar.CurrentUri.Equals(oldUri, StringComparison.OrdinalIgnoreCase);

        try
        {
            // Move/Rename the directory.
            if (hasNewUri && Directory.Exists(oldUri))
            {
                Directory.CreateDirectory(Directory.GetParent(BotDirectoryUriBar.CurrentUri)?.FullName ?? string.Empty);
                Directory.Move(oldUri, BotDirectoryUriBar.CurrentUri);
            }

            // Update the application settings.
            if (hasNewUri || !BotName.Equals(oldName, StringComparison.OrdinalIgnoreCase))
            {
                await _appConfigManager.UpdateConfigAsync(x => x.BotEntries[Position] = x.BotEntries[Position] with
                {
                    Name = BotName,
                    InstanceDirectoryUri = BotDirectoryUriBar.CurrentUri
                });
            }
        }
        catch (Exception ex)
        {
            await _mainWindow.ShowDialogWindowAsync($"An error occurred while moving/renaming {oldName}:\n{ex.Message}", DialogType.Error, Icon.Error);
        }
        finally
        {
            EnableButtons(!wereButtonsUnlocked, true);
        }
    }

    /// <summary>
    /// Opens a file in the bot directory with the default program associated with it.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <exception cref="InvalidOperationException">Occurs when there is no program available to open the specified file.</exception>
    public async ValueTask OpenFileAsync(string fileName)
    {
        try
        {
            var fileUri = Directory.EnumerateFiles(_appConfigManager.AppConfig.BotEntries[Resolver.Position].InstanceDirectoryUri, fileName, SearchOption.AllDirectories)
                .First(x => x.Contains(fileName, StringComparison.Ordinal));

            _ = Process.Start(new ProcessStartInfo()
            {
                FileName = fileUri,
                UseShellExecute = true,
            }) ?? throw new InvalidOperationException($"Failed opening {fileName}. There is no program association for files of type '{fileName[(fileName.LastIndexOf('.') + 1)..]}'.");
        }
        catch (Exception ex)
        {
            await _mainWindow.ShowDialogWindowAsync($"An error occurred while opening {fileName}:\n{ex.Message}", DialogType.Error, Icon.Error);
        }
    }

    /// <summary>
    /// Creates a backup of the bot instance associated with this view-model.
    /// </summary>
    public async ValueTask BackupBotAsync()
    {
        EnableButtons(true, false);

        var backupUri = await Resolver.CreateBackupAsync();

        await ((string.IsNullOrWhiteSpace(backupUri))
            ? _mainWindow.ShowDialogWindowAsync($"Bot {Resolver.BotName} not found.", DialogType.Error, Icon.Error)
            : _mainWindow.ShowDialogWindowAsync($"Successfully backed up {Resolver.BotName} to:{Environment.NewLine}{backupUri}", iconType: Icon.Success));

        EnableButtons(false, true);
    }

    /// <summary>
    /// Deletes the bot instance associated with this view-model.
    /// </summary>
    public async ValueTask DeleteBotAsync()
    {
        var buttonPressed = await MessageBoxManager.GetMessageBoxStandard(new()
        {
            ButtonDefinitions = ButtonEnum.OkCancel,
            ContentTitle = "Are you sure?",
            ContentMessage = $"Are you sure you want to delete {Resolver.BotName}?{Environment.NewLine}This action cannot be undone.",
            MaxWidth = int.Parse(WindowConstants.DefaultWindowWidth) / 2.0,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInCenter = true,
            WindowIcon = AppStatics.DialogWindowIcon,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        }).ShowWindowDialogAsync(_mainWindow);

        if (buttonPressed is not ButtonResult.Ok)
            return;

        EnableButtons(true, false);

        // Delete the bot instance
        var botUri = _appConfigManager.AppConfig.BotEntries[Position].InstanceDirectoryUri;

        if (Directory.Exists(botUri))
            Directory.Delete(botUri, true);

        // Update settings
        await _appConfigManager.DeleteBotEntryAsync(Position);

        // Trigger delete event
        BotDeleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Installs or updates the bot instance associated with this view-model.
    /// </summary>
    /// <param name="dependencyButton">The view-model of the dependency button that was pressed.</param>
    /// <param name="eventArgs">The event arguments.</param>
    public async Task InstallOrUpdateAsync(DependencyButtonViewModel dependencyButton, EventArgs eventArgs)
    {
        EnableButtons(true, false);

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
            _ = LoadUpdateBarAsync(Resolver, UpdateBar);

            EnableButtons(false, true);
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

    /// <summary>
    /// Locks or unlocks the settings buttons of this view-model.
    /// </summary>
    /// <param name="lockButtons">Whether the settings buttons should be locked.</param>
    /// <param name="isIdle">Whether this view-model is currently undergoing an operation.</param>
    private void EnableButtons(bool lockButtons, bool isIdle)
    {
        AreButtonsUnlocked = !lockButtons;
        IsIdle = isIdle;
    }
}