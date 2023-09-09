using Avalonia.ReactiveUI;
using NadekoHub.ViewModels.Windows;

namespace NadekoHub;

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