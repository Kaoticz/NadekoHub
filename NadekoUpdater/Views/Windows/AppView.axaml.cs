using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NadekoUpdater.DesignData.Common;
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
    /// <summary>
    /// Designer's constructor. Use the parameterized constructor instead.
    /// </summary>
    [Obsolete(WindowConstants.DesignerCtorWarning, true)]
    public AppView() : this(
            DesignStatics.Services.GetRequiredService<AppViewModel>(),
            DesignStatics.Services.GetRequiredService<LateralBarView>(),
            DesignStatics.Services.GetRequiredService<IServiceScopeFactory>()
        )
    {
    }

    /// <summary>
    /// Creates the main window of the application.
    /// </summary>
    /// <param name="viewModel">The view-model of this view.</param>
    /// <param name="lateralBarView">The lateral bar view.</param>
    /// <param name="scopeFactory">The IoC scope factory.</param>
    public AppView(AppViewModel viewModel, LateralBarView lateralBarView, IServiceScopeFactory scopeFactory)
    {
        lateralBarView.ConfigButton.Click += (_, _) => viewModel.ContentViewModel = GetViewModel<ConfigViewModel>(scopeFactory);
        lateralBarView.HomeButton.Click += (_, _) => viewModel.ContentViewModel = GetViewModel<HomeViewModel>(scopeFactory);

        this.WhenActivated(_ => base.ViewModel = viewModel);    // Sets the view-model to the one in the IoC container.
        InitializeComponent();
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
}