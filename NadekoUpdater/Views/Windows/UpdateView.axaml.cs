using Avalonia.ReactiveUI;
using NadekoUpdater.ViewModels.Windows;

namespace NadekoUpdater;

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