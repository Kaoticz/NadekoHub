using Avalonia.ReactiveUI;
using NadekoHub.Features.Common.ViewModels;

namespace NadekoHub.Features.Common.Views.Controls;

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