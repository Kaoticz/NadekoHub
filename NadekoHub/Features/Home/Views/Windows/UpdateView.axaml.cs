using Avalonia.ReactiveUI;
using NadekoHub.Features.Home.ViewModels;

namespace NadekoHub.Features.Home.Views.Windows;

/// <summary>
/// Represents the update dialog window of the application.
/// </summary>
public partial class UpdateView : ReactiveWindow<UpdateViewModel>
{
    /// <summary>
    /// Creates the update dialog window.
    /// </summary>
    public UpdateView()
        => InitializeComponent();
}