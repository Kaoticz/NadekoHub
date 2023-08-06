using Avalonia.ReactiveUI;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.Views.Controls;

/// <summary>
/// View for the home window, with buttons linking to official Nadeko resources.
/// </summary>
public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    /// <summary>
    /// Creates the home window of the application.
    /// </summary>
    public HomeView()
        => InitializeComponent();
}