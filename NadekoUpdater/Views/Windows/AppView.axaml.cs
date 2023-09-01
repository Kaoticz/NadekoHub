using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.Enums;
using NadekoUpdater.Services;
using NadekoUpdater.Services.Abstractions;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.ViewModels.Windows;
using NadekoUpdater.Views.Controls;
using ReactiveUI;
using System.Diagnostics;

namespace NadekoUpdater.Views.Windows;

/// <summary>
/// Represents the main window of the application.
/// </summary>
public partial class AppView : ReactiveWindow<AppViewModel>
{
    private Task _saveWindowSizeTask = Task.CompletedTask;
    private readonly IAppConfigManager _appConfigManager;
    private readonly IBotOrchestrator _botOrchestrator;
    private readonly ILogWriter _logWriter;

    /// <summary>
    /// Designer's constructor. Use the parameterized constructor instead.
    /// </summary>
    [Obsolete(WindowConstants.DesignerCtorWarning, true)]
    public AppView() : this(
            DesignStatics.Services.GetRequiredService<IServiceScopeFactory>(),
            DesignStatics.Services.GetRequiredService<IBotOrchestrator>(),
            DesignStatics.Services.GetRequiredService<ILogWriter>(),
            DesignStatics.Services.GetRequiredService<IAppConfigManager>(),
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
    /// <param name="viewModel">The view-model of this view.</param>
    /// <param name="lateralBarView">The lateral bar view.</param>
    public AppView(IServiceScopeFactory scopeFactory, IBotOrchestrator botOrchestrator, ILogWriter logWriter,
        IAppConfigManager appConfigManager, AppViewModel viewModel, LateralBarView lateralBarView)
    {
        // Shorthand for fetching the bot button from the lateral bar structure
        static Button GetBarButton(Control border)
            => (((border as Border)?.Child as Panel)?.Children[1] as Button) ?? throw new InvalidOperationException("Unexpected layout.");

        _appConfigManager = appConfigManager;
        _botOrchestrator = botOrchestrator;
        _logWriter = logWriter;

        lateralBarView.ConfigButton.Click += (_, _) =>
        {
            lateralBarView.ResetBotButtonBorders();
            viewModel.ContentViewModel = GetViewModel<ConfigViewModel>(scopeFactory);
        };

        lateralBarView.HomeButton.Click += (_, _) =>
        {
            lateralBarView.ResetBotButtonBorders();
            viewModel.ContentViewModel = GetViewModel<HomeViewModel>(scopeFactory);
        };

        lateralBarView.BotButtonClick += (button, _) =>
        {
            // If the user clicked on the bot instance that is already active, exit.
            if (base.ViewModel?.ContentViewModel is BotConfigViewModel currentViewModel && currentViewModel.Id.Equals(button.Content))
                return;

            // Switch to the bot config view-model
            var botConfigViewModel = GetBotConfigViewModel(button, scopeFactory);
            viewModel.ContentViewModel = botConfigViewModel;

            // Update the selector on the lateral bar
            lateralBarView.ResetBotButtonBorders();
            lateralBarView.ApplyBotButtonBorder(button);

            // Update the avatar on the lateral bar.
            botConfigViewModel.AvatarChanged += (_, eventArgs) => lateralBarView.UpdateBotButtonAvatarAsync(eventArgs);

            // If the bot instance is deleted, load the Home view.
            botConfigViewModel.BotDeleted += async (bcvm, _) =>
            {
                lateralBarView.ResetBotButtonBorders();
                viewModel.ContentViewModel = GetViewModel<HomeViewModel>(scopeFactory);
                await viewModel.LateralBarInstance.RemoveBotButtonAsync(botConfigViewModel.Id);

                // Fix weird bug that redraws bot buttons with the wrong avatars.
                // A random border with null button content just pops up randomly in ButtonList.Children.
                // This probably happens because the view references a Border with a bunch of stuff, whereas the view-model only
                // references a Button, but I can't be bothered to do this the right way. 
                lateralBarView.ButtonList.Children.RemoveAll(lateralBarView.ButtonList.Children.Where(x => GetBarButton(x).Content is null));
            };
        };

        this.WhenActivated(_ => base.ViewModel = viewModel);    // Sets the view-model to the one in the IoC container.
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnResized(WindowResizedEventArgs eventArgs)
    {
        if (base.IsLoaded && _saveWindowSizeTask.IsCompleted)
            _saveWindowSizeTask = SaveCurrentWindowSizeAsync(TimeSpan.FromSeconds(1));

        base.OnResized(eventArgs);
    }

    /// <inheritdoc/>
    /// <exception cref="UnreachableException">Occurs when <see cref="ThemeType"/> has an unimplemented value.</exception>
    protected override void OnOpened(EventArgs eventArgs)
    {
        // Ensure that bots on Unix system have access to the dependencies.
        if (Environment.OSVersion.Platform is PlatformID.Unix)
            Utilities.AddPathToPATHEnvar(AppStatics.AppDepsUri);

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
    /// Gets a <typeparamref name="T"/> from the <paramref name="scopeFactory"/>.
    /// </summary>
    /// <typeparam name="T">The type of view-model to be returned.</typeparam>
    /// <param name="scopeFactory">The IoC scope factory.</param>
    /// <returns>A <typeparamref name="T"/>.</returns>
    private static T GetViewModel<T>(IServiceScopeFactory scopeFactory) where T : ViewModelBase
    {
        using var scope = scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a <see cref="BotConfigViewModel"/> from the <paramref name="scopeFactory"/> and initializes
    /// its properties with user data.
    /// </summary>
    /// <param name="button">The button that was pressed in the bot list.</param>
    /// <param name="scopeFactory">The IoC scope factory.</param>
    /// <returns>The view-model associated with the pressed <paramref name="button"/>.</returns>
    /// <exception cref="InvalidCastException">Occurs when <paramref name="button"/> has a <see cref="ContentControl.Content"/> that is not an <see langword="uint"/>.</exception>
    /// <exception cref="InvalidOperationException">Occurs when <paramref name="button"/> has an invalid <see cref="ContentControl.Content"/>.</exception>
    private static BotConfigViewModel GetBotConfigViewModel(Button button, IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var botId = (Guid)(button.Content ?? throw new InvalidOperationException("Bot button has no valid Id."));
        var botResolver = scope.ServiceProvider.GetParameterizedService<NadekoResolver>(botId);

        return scope.ServiceProvider.GetParameterizedService<BotConfigViewModel>(botResolver);
    }
}