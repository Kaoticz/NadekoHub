using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using NadekoHub.Avalonia.DesignData.Common;
using NadekoHub.Enums;
using NadekoHub.Features.Abstractions;
using NadekoHub.Features.AppConfig.Services.Abstractions;
using NadekoHub.Features.AppConfig.ViewModels;
using NadekoHub.Features.AppWindow.Models;
using NadekoHub.Features.AppWindow.ViewModels;
using NadekoHub.Features.AppWindow.Views.Controls;
using NadekoHub.Features.BotConfig.Services.Abstractions;
using NadekoHub.Features.BotConfig.ViewModels;
using NadekoHub.Features.Home.Services.Abstractions;
using NadekoHub.Features.Home.ViewModels;
using NadekoHub.Features.Home.Views.Windows;
using NadekoHub.Features.BotConfig.Services;
using ReactiveUI;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;

namespace NadekoHub.Features.AppWindow.Views.Windows;

/// <summary>
/// Represents the main window of the application.
/// </summary>
public partial class AppView : ReactiveWindow<AppViewModel>
{
    private Task _saveWindowSizeTask = Task.CompletedTask;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAppConfigManager _appConfigManager;
    private readonly IBotOrchestrator _botOrchestrator;
    private readonly ILogWriter _logWriter;
    private readonly IAppResolver _appResolver;
    private readonly LateralBarView _lateralBarView;

    /// <summary>
    /// Designer's constructor. Use the parameterized constructor instead.
    /// </summary>
    [Obsolete(WindowConstants.DesignerCtorWarning, true)]
    public AppView() : this(
            DesignStatics.Services.GetRequiredService<IServiceScopeFactory>(),
            DesignStatics.Services.GetRequiredService<IBotOrchestrator>(),
            DesignStatics.Services.GetRequiredService<ILogWriter>(),
            DesignStatics.Services.GetRequiredService<IAppConfigManager>(),
            DesignStatics.Services.GetRequiredService<IAppResolver>(),
            DesignStatics.Services.GetRequiredService<AppViewModel>(),
            DesignStatics.Services.GetRequiredService<LateralBarView>()
        )
    {
    }

    /// <summary>
    /// Creates the main window of the application.
    /// </summary>
    /// <param name="scopeFactory">The IoC scope factory.</param>
    /// <param name="botOrchestrator">The bot orchestrator.</param>
    /// <param name="logWriter">The service responsible for creating log files.</param>
    /// <param name="appConfigManager">The manager for the application settings.</param>
    /// <param name="appResolver">The service that updates this application.</param>
    /// <param name="viewModel">The view-model of this view.</param>
    /// <param name="lateralBarView">The lateral bar view.</param>
    public AppView(IServiceScopeFactory scopeFactory, IBotOrchestrator botOrchestrator, ILogWriter logWriter,
        IAppConfigManager appConfigManager, IAppResolver appResolver, AppViewModel viewModel, LateralBarView lateralBarView)
    {
        _scopeFactory = scopeFactory;
        _appConfigManager = appConfigManager;
        _botOrchestrator = botOrchestrator;
        _logWriter = logWriter;
        _lateralBarView = lateralBarView;
        _appResolver = appResolver;

        _lateralBarView.ConfigButton.Click += (_, _) =>
        {
            _lateralBarView.ResetBotButtonBorders();
            viewModel.ContentViewModel = GetViewModel<ConfigViewModel>();
        };

        _lateralBarView.HomeButton.Click += (_, _) =>
        {
            _lateralBarView.ResetBotButtonBorders();
            viewModel.ContentViewModel = GetViewModel<HomeViewModel>();
        };

        _lateralBarView.BotButtonClick += (button, _) =>
        {
            var botConfigViewModel = SwitchBotConfigViewModel(button, _lateralBarView);

            if (botConfigViewModel is null)
                return;

            // Update the avatar on the lateral bar.
            botConfigViewModel.AvatarChanged += (_, eventArgs) => _lateralBarView.UpdateBotButtonAvatarAsync(eventArgs);

            // If the bot instance is deleted, load the Home view.
            botConfigViewModel.BotDeleted += (bcvm, _) => RemoveBotAsync(bcvm.Id, _lateralBarView);
        };

        this.WhenActivated(_ => base.ViewModel = viewModel);    // Sets the view-model to one from the IoC container.
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnResized(WindowResizedEventArgs eventArgs)
    {
        if (base.IsLoaded && _saveWindowSizeTask.IsCompleted)
            _saveWindowSizeTask = SaveCurrentWindowSizeAsync(TimeSpan.FromSeconds(1));

        base.OnResized(eventArgs);
    }

    /// <inheritdoc />
    /// <exception cref="UnreachableException">Occurs when <see cref="ThemeType"/> has an unimplemented value.</exception>
    protected override void OnOpened(EventArgs eventArgs)
    {
        // Ensure that bots on Unix system have access to the dependencies.
        if (Environment.OSVersion.Platform is PlatformID.Unix)
            Utilities.AddPathToPathEnvar(AppStatics.AppDepsUri);

        // Set the window size from the last session
        base.Height = _appConfigManager.AppConfig.WindowSize.Height;
        base.Width = _appConfigManager.AppConfig.WindowSize.Width;

        // Set the user prefered theme
        base.RequestedThemeVariant = _appConfigManager.AppConfig.Theme switch
        {
            ThemeType.Auto => ThemeVariant.Default,
            ThemeType.Light => ThemeVariant.Light,
            ThemeType.Dark => ThemeVariant.Dark,
            _ => throw new UnreachableException($"No implementation for theme of type {_appConfigManager.AppConfig.Theme} was provided."),
        };

        // Update the application, if one is available
        _ = UpdateAndCloseAsync();

        // Import bots from the old updater, if available
        if (OperatingSystem.IsWindows())
            _ = MigrateOldBotsAsync();

        base.OnOpened(eventArgs);
    }

    /// <inheritdoc />
    protected override void OnClosing(WindowClosingEventArgs eventArgs)
    {
        // Hide the window instead of closing it, in case the user
        // prefers the window to be minimized to the system tray.
        eventArgs.Cancel = _appConfigManager.AppConfig.MinimizeToTray && !eventArgs.IsProgrammatic;

        if (eventArgs.Cancel)
            base.Hide();

        base.OnClosing(eventArgs);
    }

    /// <inheritdoc/>
    protected override async void OnClosed(EventArgs eventArgs)
    {
        // When the updater is closed, kill all bots and write their logs.
        _botOrchestrator.StopAll();
        await _logWriter.FlushAllAsync(true);

        base.OnClosed(eventArgs);
    }

    /// <summary>
    /// Saves the size of this window to the application settings after the specified <paramref name="waitTime"/> elapses.
    /// </summary>
    /// <param name="waitTime">How long to wait before saving the window size to the settings.</param>
    /// <param name="cToken">The cancellation token.</param>
    private async Task SaveCurrentWindowSizeAsync(TimeSpan waitTime, CancellationToken cToken = default)
    {
        await Task.Delay(waitTime, cToken);
        await _appConfigManager.UpdateConfigAsync(x => x.WindowSize = new(base.Width, base.Height), cToken);
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/> from the <see cref="_scopeFactory"/>.
    /// </summary>
    /// <typeparam name="T">The type of view-model to be returned.</typeparam>
    /// <returns>A <typeparamref name="T"/>.</returns>
    private T GetViewModel<T>() where T : ViewModelBase
    {
        using var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Removes the bot with the specified Id from the settings file and the lateral bar.
    /// </summary>
    /// <param name="botId">The Id of the bot to be removed.</param>
    /// <param name="lateralBarView">The view of the lateral bar.</param>
    /// <exception cref="InvalidOperationException">Occurs when the lateral bar has an unexpected structure layout.</exception>
    private async Task RemoveBotAsync(Guid botId, LateralBarView lateralBarView)
    {
        // Shorthand for fetching the bot button from the lateral bar structure
        static Button GetBarButton(Control border)
            => (((border as Border)?.Child as Panel)?.Children[1] as Button) ?? throw new InvalidOperationException("Unexpected layout.");

        base.ViewModel ??= GetViewModel<AppViewModel>();

        lateralBarView.ResetBotButtonBorders();
        base.ViewModel.ContentViewModel = GetViewModel<HomeViewModel>();

        if (lateralBarView.ViewModel is not null)
            await lateralBarView.ViewModel.RemoveBotButtonAsync(botId);

        // Fix weird bug that redraws bot buttons with the wrong avatars.
        // A random border with null button content just pops up randomly in ButtonList.Children.
        // This probably happens because the view references a Border with a bunch of stuff, whereas the view-model only
        // references a Button, but I can't be bothered to do this the right way. 
        lateralBarView.ButtonList.Children.RemoveAll(lateralBarView.ButtonList.Children.Where(x => GetBarButton(x).Content is null));
    }

    /// <summary>
    /// Switches to the view-model associated with the specified <paramref name="button"/> and updates
    /// the selection border on the lateral bar.
    /// </summary>
    /// <param name="button">The bot button that was pressed.</param>
    /// <param name="lateralBarView">The view of the lateral bar.</param>
    /// <returns>
    /// The view-model associated with <paramref name="button"/>,
    /// <see langword="null"/> if the current and requested view-models are the same.
    /// </returns>
    private BotConfigViewModel? SwitchBotConfigViewModel(Button button, LateralBarView lateralBarView)
    {
        // If the user clicked on the bot instance that is already active, do not switch.
        if (base.ViewModel?.ContentViewModel is BotConfigViewModel currentViewModel && currentViewModel.Id.Equals(button.Content))
            return default;

        // Switch to the bot config view-model
        var botConfigViewModel = GetBotConfigViewModel(button, _scopeFactory);
        this.ViewModel ??= GetViewModel<AppViewModel>();
        this.ViewModel.ContentViewModel = botConfigViewModel;

        // Update the selector on the lateral bar
        lateralBarView.ResetBotButtonBorders();
        lateralBarView.ApplyBotButtonBorder(button);

        return botConfigViewModel;
    }

    /// <summary>
    /// Updates this application, if a new version is available.
    /// </summary>
    private async Task UpdateAndCloseAsync()
    {
        _appResolver.RemoveOldFiles();

        if (!_appConfigManager.AppConfig.AutomaticUpdates || await _appResolver.CanUpdateAsync() is not true)
            return;

        _ = new UpdateView().ShowDialog(this);

        await _appResolver.InstallOrUpdateAsync(AppContext.BaseDirectory);
        _appResolver.LaunchNewVersion();

        base.Close();
    }

    /// <summary>
    /// Migrates bots created by the NadekoUpdater when NadekoHub is run for the first time.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private async Task MigrateOldBotsAsync()
    {
        var configFileUri = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "NadekoBotUpdater", "bots.json");

        if (File.Exists(AppStatics.AppConfigUri) || !File.Exists(configFileUri))
            return;

        var bots = (JsonSerializer.Deserialize<OldUpdaterBotEntry[]>(await File.ReadAllTextAsync(configFileUri)) ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.PathUri) && File.Exists(Path.Combine(x.PathUri, "NadekoBot.exe")))
            .Select((x, y) => new BotEntry(x.Guid, new(x.Name, x.PathUri!, (uint)y, x.Version, x.IconUri)));

        foreach (var botEntry in bots)
            await _appConfigManager.UpdateConfigAsync(x => x.BotEntries.TryAdd(botEntry.Id, botEntry.BotInfo));

        _lateralBarView.ViewModel?.ReloadBotButtons(_appConfigManager.AppConfig.BotEntries);
    }

    /// <summary>
    /// Gets a <see cref="BotConfigViewModel"/> from the <paramref name="scopeFactory"/> and initializes
    /// its properties with user data.
    /// </summary>
    /// <param name="button">The button that was pressed in the bot list.</param>
    /// <param name="scopeFactory">The IoC scope factory.</param>
    /// <returns>The view-model associated with the pressed <paramref name="button"/>.</returns>
    /// <exception cref="InvalidCastException">Occurs when <paramref name="button"/> has a <see cref="ContentControl.Content"/> that is not a <see cref="Guid"/>.</exception>
    /// <exception cref="InvalidOperationException">Occurs when <paramref name="button"/> has an invalid <see cref="ContentControl.Content"/>.</exception>
    private static BotConfigViewModel GetBotConfigViewModel(Button button, IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var botId = button.Content ?? throw new InvalidOperationException("Bot button has no valid Id.");
        var botResolver = scope.ServiceProvider.GetParameterizedService<NadekoResolver>(botId);

        return scope.ServiceProvider.GetParameterizedService<BotConfigViewModel>(botResolver);
    }
}