using Avalonia.ReactiveUI;
using NadekoUpdater.ViewModels.Controls;

namespace NadekoUpdater.Views.Controls;

/// <summary>
/// Represents a button that installs a dependency for Nadeko.
/// </summary>
public partial class DependencyButton : ReactiveUserControl<DependencyButtonViewModel>
{
    /// <summary>
    /// Creates a button that installs a dependency for Nadeko.
    /// </summary>
    public DependencyButton()
        => InitializeComponent();
}