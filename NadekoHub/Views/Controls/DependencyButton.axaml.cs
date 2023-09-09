using Avalonia.ReactiveUI;
using NadekoHub.ViewModels.Controls;

namespace NadekoHub.Views.Controls;

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