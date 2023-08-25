using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.Models.Config;
using NadekoUpdater.Services;
using NadekoUpdater.Services.Abstractions;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.ViewModels.Windows;
using NadekoUpdater.Views.Controls;
using ReactiveUI;

namespace NadekoUpdater.Views.Windows;

/// <summary>
/// Represents the main window of the application.
/// </summary>
public partial class AppView : ReactiveWindow<AppViewModel>
{
    private readonly ReadOnlyAppConfig _appConfig;
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
            DesignStatics.Services.GetRequiredService<ReadOnlyAppConfig>(),
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
    /// <param name="appConfig">The application settings.</param>
    /// <param name="viewModel">The view-model of this view.</param>
    /// <param name="lateralBarView">The lateral bar view.</param>
    public AppView(IServiceScopeFactory scopeFactory, IBotOrchestrator botOrchestrator, ILogWriter logWriter,
        ReadOnlyAppConfig appConfig, AppViewModel viewModel, LateralBarView lateralBarView)
    {
        _appConfig = appConfig;
        _botOrchestrator = botOrchestrator;
        _logWriter = logWriter;

        lateralBarView.ConfigButton.Click += (_, _) => viewModel.ContentViewModel = GetViewModel<ConfigViewModel>(scopeFactory);
        lateralBarView.HomeButton.Click += (_, _) => viewModel.ContentViewModel = GetViewModel<HomeViewModel>(scopeFactory);
        lateralBarView.BotButtonClick += (button, _) =>
        {
            var botConfigViewModel = GetBotConfigViewModel(button, scopeFactory);
            viewModel.ContentViewModel = botConfigViewModel;

            // If the bot instance is deleted, load the Home view.
            botConfigViewModel.BotDeleted += async (_, _) =>
            {
                viewModel.ContentViewModel = GetViewModel<HomeViewModel>(scopeFactory);
                await viewModel.LateralBarInstance.RemoveBotButtonAsync(botConfigViewModel.Position);
            };
        };

        this.WhenActivated(_ => base.ViewModel = viewModel);    // Sets the view-model to the one in the IoC container.
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnOpened(EventArgs eventArgs)
    {
        // Ensure that bots on Unix system have access to the dependencies.
        if (Environment.OSVersion.Platform is PlatformID.Unix)
            Utilities.AddPathToPATHEnvar(AppStatics.AppDepsUri);

        base.OnOpened(eventArgs);
    }

    /// <inheritdoc />
    protected override void OnClosing(WindowClosingEventArgs eventArgs)
    {
        // Hide the window instead of closing it, in case the user
        // prefers the window to be minimized to the system tray.
        eventArgs.Cancel = _appConfig.MinimizeToTray && !eventArgs.IsProgrammatic;

        if (eventArgs.Cancel)
            base.Hide();

        base.OnClosing(eventArgs);
    }

    /// <inheritdoc/>
    protected override async void OnClosed(EventArgs e)
    {
        // When the updater is closed, kill all bots and write their logs.
        _botOrchestrator.StopAll();
        await _logWriter.FlushAllAsync(true);

        base.OnClosed(e);
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
        var position = (uint)(button.Content ?? throw new InvalidOperationException("Bot button has no id/position."));
        var botResolver = scope.ServiceProvider.GetParameterizedService<NadekoResolver>(position);

        return scope.ServiceProvider.GetParameterizedService<BotConfigViewModel>(botResolver);
    }
}