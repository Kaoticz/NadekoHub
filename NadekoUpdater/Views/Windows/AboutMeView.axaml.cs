using Avalonia.ReactiveUI;
using NadekoUpdater.ViewModels.Windows;

namespace NadekoUpdater.Views.Windows;

/// <summary>
/// Represents the about me dialog window.
/// </summary>
public partial class AboutMeView : ReactiveWindow<AboutMeViewModel>
{
    /// <summary>
    /// Creates the about me dialog window.
    /// </summary>
    public AboutMeView()
        => InitializeComponent();
}