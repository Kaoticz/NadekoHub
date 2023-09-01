using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NadekoUpdater.Enums;
using NadekoUpdater.Models.EventArguments;
using NadekoUpdater.Services.Abstractions;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using NadekoUpdater.Views.Windows;
using ReactiveUI;
using SkiaSharp;
using System.Diagnostics;
using System.Reactive.Disposables;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// View-model for <see cref="BotConfigView"/>, the window with settings and controls for a specific bot instance.
/// </summary>
public class BotConfigViewModel : ViewModelBase<BotConfigView>, IDisposable
{
    private string _botName = string.Empty;
    private string _directoryHint = string.Empty;
    private bool _isBotRunning;
    private bool _isIdle;
    private bool _areButtonsUnlocked;
    private SKBitmap _botAvatar;
    private readonly IAppConfigManager _appConfigManager;
    private readonly AppView _mainWindow;
    private readonly IBotOrchestrator _botOrchestrator;
    private readonly ILogWriter _logWriter;

    /// <summary>
    /// Raised when the user deletes the bot instance associated with this view-model.
    /// </summary>
    public event AsyncEventHandler<BotConfigViewModel, EventArgs>? BotDeleted;

    /// <summary>
    /// Raised when the user sets a new avatar for the bot instance associated with this view-model.
    /// </summary>
    public event AsyncEventHandler<BotConfigViewModel, AvatarChangedEventArgs>? AvatarChanged;

    /// <summary>
    /// The bot resolver to be used.
    /// </summary>
    public IBotResolver Resolver { get; }

    /// <summary>
    /// The Id of the bot associated with this view-model.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The bar that defines where the bot instance should be saved to.
    /// </summary>
    public UriInputBarViewModel BotDirectoryUriBar { get; }

    /// <summary>
    /// The bar to update the bot instance.
    /// </summary>
    public DependencyButtonViewModel UpdateBar { get; }

    /// <summary>
    /// The fake console that displays the bot's output.
    /// </summary>
    public FakeConsoleViewModel FakeConsole { get; }

    /// <summary>
    /// The bot avatar to be displayed on the front-end.
    /// </summary>
    public SKBitmap BotAvatar
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
    /// Determines whether the bot associated with this view-model is running or not.
    /// </summary>
    public bool IsBotRunning
    {
        get => _isBotRunning;
        private set => this.RaiseAndSetIfChanged(ref _isBotRunning, value);
    }

    /// <summary>
    /// Creates a view-model for <see cref="BotConfigView"/>.
    /// </summary>
    /// <param name="appConfigManager">The app settings manager.</param>
    /// <param name="mainWindow">The main window of the application.</param>
    /// <param name="botDirectoryUriBar">The text box with the path to the directory where the bot instance is.</param>
    /// <param name="updateBotBar">The bar for updating the bot.</param>
    /// <param name="fakeConsole">The fake console to write the bot's output to.</param>
    /// <param name="botResolver">The bot resolver to be used.</param>
    /// <param name="botOrchestrator">The bot orchestrator.</param>
    /// <param name="logWriter">The service responsible for creating log files.</param>
    public BotConfigViewModel(IAppConfigManager appConfigManager, AppView mainWindow, UriInputBarViewModel botDirectoryUriBar, DependencyButtonViewModel updateBotBar,
        FakeConsoleViewModel fakeConsole, IBotResolver botResolver, IBotOrchestrator botOrchestrator, ILogWriter logWriter)
    {
        _appConfigManager = appConfigManager;
        _mainWindow = mainWindow;
        _botOrchestrator = botOrchestrator;
        _logWriter = logWriter;
        BotDirectoryUriBar = botDirectoryUriBar;
        UpdateBar = updateBotBar;
        FakeConsole = fakeConsole;

        UpdateBar.Click += InstallOrUpdateAsync;
        _botOrchestrator.OnStdout += WriteLog;
        _botOrchestrator.OnStderr += WriteLog;
        _botOrchestrator.OnBotExit += LogBotExit;
        _botOrchestrator.OnBotExit += ReenableButtonsOnBotExit;

        var botEntry = _appConfigManager.AppConfig.BotEntries[botResolver.Id];

        _logWriter.TryRead(botResolver.Id, out var logContent);
        FakeConsole.Content = logContent ?? string.Empty;
        FakeConsole.Watermark = "Waiting for the bot to start...";
        Resolver = botResolver;
        BotDirectoryUriBar.CurrentUri = botEntry.InstanceDirectoryUri;
        _botAvatar = Utilities.LoadLocalImage(botEntry.AvatarUri);
        BotName = botResolver.BotName;
        Id = botResolver.Id;
        UpdateBar.DependencyName = "Checking...";
        IsBotRunning = botOrchestrator.IsBotRunning(botResolver.Id);

        if (IsBotRunning)
            EnableButtons(true, false);
        else
            EnableButtons(!Directory.Exists(botEntry.InstanceDirectoryUri), true);

        _ = LoadUpdateBarAsync(botResolver, updateBotBar);

        // Dispose when the view is deactivated
        this.WhenActivated(disposables => Disposable.Create(() => Dispose()).DisposeWith(disposables));
    }

    /// <summary>
    /// Moves or renames the bot instance associated with this view-model.
    /// </summary>
    public async ValueTask MoveOrRenameAsync()
    {
        var wereButtonsUnlocked = AreButtonsUnlocked;
        EnableButtons(true, false);

        var botEntry = _appConfigManager.AppConfig.BotEntries[Resolver.Id];
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
                await _appConfigManager.UpdateBotEntryAsync(Id, x => x with
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
            var fileUri = Directory.EnumerateFiles(_appConfigManager.AppConfig.BotEntries[Resolver.Id].InstanceDirectoryUri, fileName, SearchOption.AllDirectories)
                .First(x => x.Contains(fileName, StringComparison.Ordinal));

            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = fileUri,
                UseShellExecute = true,
            }) ?? throw new InvalidOperationException($"Failed opening {fileName}. There is no program association for files of type '{fileName[(fileName.LastIndexOf('.') + 1)..]}'.");

            process.EnableRaisingEvents = true;
            process.Exited += (sender, _) => (sender as Process)?.Dispose();
        }
        catch (Exception ex)
        {
            await _mainWindow.ShowDialogWindowAsync($"An error occurred while opening {fileName}:\n{ex.Message}", DialogType.Error, Icon.Error);
        }
    }

    /// <summary>
    /// Associates an avatar to the bot instance of this view-model.
    /// </summary>
    public async ValueTask SaveAvatarAsync()
    {
        var imageFileStorage = await _mainWindow.StorageProvider.OpenFilePickerAsync(AppStatics.ImageFilePickerOptions);

        if (imageFileStorage.Count is 0)
            return;

        // Save the Uri to the image file
        var imageUri = imageFileStorage[0].Path.LocalPath;

        await _appConfigManager.UpdateBotEntryAsync(Resolver.Id, x => x with { AvatarUri = imageUri });

        // Set the new avatar
        BotAvatar.Dispose();
        BotAvatar = Utilities.LoadLocalImage(imageUri);

        // Invoke event
        AvatarChanged?.Invoke(this, new(Id, BotAvatar, imageUri));
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

        // Stop the bot instance
        _botOrchestrator.Stop(Resolver.Id);

        // Cleanup
        FakeConsole.Content = string.Empty;
        await _logWriter.FlushAsync(Resolver.Id, true);

        UpdateBar.Click -= InstallOrUpdateAsync;
        _botOrchestrator.OnStdout -= WriteLog;
        _botOrchestrator.OnStderr -= WriteLog;
        _botOrchestrator.OnBotExit -= LogBotExit;
        _botOrchestrator.OnBotExit -= ReenableButtonsOnBotExit;

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
        if (_botOrchestrator.IsBotRunning(Resolver.Id))
        {
            await _mainWindow.ShowDialogWindowAsync("Please, stop the bot before updating it.", DialogType.Warning, Icon.Warning);
            return;
        }

        EnableButtons(true, false);

        var originalStatus = UpdateBar.Status;
        UpdateBar.Status = DependencyStatus.Updating;

        try
        {
            var dialogWindowTask = await Resolver.InstallOrUpdateAsync(_appConfigManager.AppConfig.BotEntries[Resolver.Id].InstanceDirectoryUri) switch
            {
                (string oldVer, null) => _mainWindow.ShowDialogWindowAsync($"{Resolver.DependencyName} is already up-to-date (v{oldVer})."),
                (null, string newVer) => _mainWindow.ShowDialogWindowAsync($"{Resolver.DependencyName} v{newVer} was successfully installed.", iconType: Icon.Success),
                (string oldVer, string newVer) => _mainWindow.ShowDialogWindowAsync($"{Resolver.DependencyName} was successfully updated from version {oldVer} to version {newVer}.", iconType: Icon.Success),
                (null, null) => _mainWindow.ShowDialogWindowAsync($"Update of {Resolver.DependencyName} is ongoing.", DialogType.Warning, Icon.Warning)
            };

            await dialogWindowTask;
            _ = LoadUpdateBarAsync(Resolver, UpdateBar);

            BotDirectoryUriBar.RecheckCurrentUri();
            EnableButtons(false, true);
        }
        catch (Exception ex)
        {
            await _mainWindow.ShowDialogWindowAsync($"An error occurred while updating {Resolver.DependencyName}:\n{ex.Message}", DialogType.Error, Icon.Error);
            UpdateBar.Status = originalStatus;
        }
    }

    /// <summary>
    /// Starts the bot instance associated with this view-model.
    /// </summary>
    public void StartBot()
    {
        IsBotRunning = _botOrchestrator.Start(Id);

        if (IsBotRunning)
            EnableButtons(true, false);
    }

    /// <summary>
    /// Stops the bot instance associated with this view-model.
    /// </summary>
    public void StopBot()
        => _botOrchestrator.Stop(Id);

    /// <summary>
    /// Loads the bot update bar.
    /// </summary>
    /// <param name="botResolver">The bot resolver.</param>
    /// <param name="updateBotBar">The update bar.</param>
    private async static Task LoadUpdateBarAsync(IBotResolver botResolver, DependencyButtonViewModel updateBotBar)
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
    /// Locks or unlocks the settings buttons of this view-model.
    /// </summary>
    /// <param name="lockButtons">Whether the settings buttons should be locked.</param>
    /// <param name="isIdle">Whether this view-model is currently undergoing an operation.</param>
    private void EnableButtons(bool lockButtons, bool isIdle)
    {
        AreButtonsUnlocked = !lockButtons;
        IsIdle = isIdle;
    }

    /// <summary>
    /// Writes stdout and stderr to the fake console and the log writer.
    /// </summary>
    /// <param name="botOrchestrator">The bot orchestrator.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void WriteLog(IBotOrchestrator botOrchestrator, ProcessStdWriteEventArgs eventArgs)
    {
        if (eventArgs.Id != Resolver.Id)
            return;

        _logWriter.TryAdd(eventArgs.Id, eventArgs.Output);

        FakeConsole.Content = (FakeConsole.Content.Length > 100_000)
            ? FakeConsole.Content[FakeConsole.Content.IndexOf(Environment.NewLine, 60_000)..] + eventArgs.Output + Environment.NewLine
            : FakeConsole.Content + eventArgs.Output + Environment.NewLine;
    }

    /// <summary>
    /// Logs when the bot associated with this view-model stops executing.
    /// </summary>
    /// <param name="botOrchestrator">The bot orchestrator.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void LogBotExit(IBotOrchestrator botOrchestrator, BotExitEventArgs eventArgs)
    {
        if (eventArgs.Id != Resolver.Id)
            return;

        var message = Environment.NewLine + Resolver.BotName + " stopped." + Environment.NewLine;

        _logWriter.TryAdd(Resolver.Id, message);
        FakeConsole.Content += message;
    }

    /// <summary>
    /// Reenables the buttons when the bot instance associated with this view-model exits.
    /// </summary>
    /// <param name="botOrchestrator">The bot orchestrator.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void ReenableButtonsOnBotExit(IBotOrchestrator botOrchestrator, BotExitEventArgs eventArgs)
    {
        if (eventArgs.Id != Resolver.Id)
            return;

        IsBotRunning = false;
        EnableButtons(false, true);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        BotAvatar?.Dispose();
        GC.SuppressFinalize(this);
    }
}