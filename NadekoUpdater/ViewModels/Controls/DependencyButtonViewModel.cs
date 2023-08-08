using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// Defines the view-model for a button that installs a dependency for Nadeko.
/// </summary>
public class DependencyButtonViewModel : ViewModelBase<DependencyButton>
{
    /// <summary>
    /// The name of the dependency.
    /// </summary>
    public string DependencyName { get; set; } = "DependencyName";

    /// <summary>
    /// The status of this dependency.
    /// </summary>
    /// <remarks>Install or Installed.</remarks>
    public string Status { get; set; } = "Install";
}
