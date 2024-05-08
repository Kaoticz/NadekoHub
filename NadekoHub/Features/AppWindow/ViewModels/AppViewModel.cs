using NadekoHub.Features.Abstractions;
using NadekoHub.Features.AppWindow.Views.Controls;
using NadekoHub.Features.AppWindow.Views.Windows;
using NadekoHub.Features.Home.ViewModels;
using ReactiveUI;

namespace NadekoHub.Features.AppWindow.ViewModels;

/// <summary>
/// View-model for the main window.
/// </summary>
public class AppViewModel : ViewModelBase<AppView>
{
    private ViewModelBase _contentViewModel;
    private LateralBarViewModel _lateralBarInstance;

    /// <summary>
    /// View-model instance of a <see cref="LateralBarView"/>.
    /// </summary>
    public LateralBarViewModel LateralBarInstance
    {
        get => _lateralBarInstance;
        set => this.RaiseAndSetIfChanged(ref _lateralBarInstance, value);
    }

    /// <summary>
    /// View-model instance of the view to be displayed.
    /// </summary>
    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    /// <summary>
    /// Initializes the view-model for the main window.
    /// </summary>
    /// <param name="lateralBarInstance">The lateral bar.</param>
    /// <param name="homeViewModel">The first view-model to be displayed when the application is started.</param>
    public AppViewModel(LateralBarViewModel lateralBarInstance, HomeViewModel homeViewModel)
    {
        _lateralBarInstance = lateralBarInstance;
        _contentViewModel = homeViewModel;
    }
}