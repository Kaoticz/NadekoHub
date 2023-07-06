using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NadekoUpdater.DesignData.Common;
using NadekoUpdater.ViewModels.Windows;
using NadekoUpdater.Views.Controls;
using ReactiveUI;

namespace NadekoUpdater.Views.Windows;

/// <summary>
/// Represents the main window of the application.
/// </summary>
public partial class HomeView : ReactiveWindow<HomeViewModel>
{
    /// <summary>
    /// Designer's constructor. Use the parameterized constructor instead.
    /// </summary>
    [Obsolete(WindowConstants.DesignerCtorWarning, true)]
    public HomeView() : this(
            DesignStatics.Services.GetRequiredService<LateralBarView>(),
            DesignStatics.Services.GetRequiredService<HomeViewModel>()
        )
    {
    }

    /// <summary>
    /// Creates the main window of the application.
    /// </summary>
    /// <param name="lateralBarView">The view for the lateral bar.</param>
    /// <param name="viewModel">The view-model of this view.</param>
    public HomeView(LateralBarView lateralBarView, HomeViewModel viewModel)
    {
        lateralBarView.ConfigButton.Click += (_, _) => ShowDialogWindow();
        this.WhenActivated(_ => base.ViewModel = viewModel);    // Replace the HomeViewModel made by Avalonia with the one in the IoC container

        InitializeComponent();
    }

    /// <summary>
    /// For testing purposes - to be removed!
    /// </summary>
    public Task ShowDialogWindow()
    {
        var dialogBox = MessageBoxManager.GetMessageBoxStandard(
            DialogType.Notification.ToString(),
            $"Hello from {nameof(HomeView)} view!",
            ButtonEnum.Ok,
            MsBox.Avalonia.Enums.Icon.Success,
            WindowStartupLocation.CenterOwner
        );

        return dialogBox.ShowWindowDialogAsync(this);
    }
}