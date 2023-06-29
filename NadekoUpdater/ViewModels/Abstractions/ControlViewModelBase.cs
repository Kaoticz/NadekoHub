using Avalonia.Controls;

namespace NadekoUpdater.ViewModels.Abstractions;

/// <summary>
/// Base view-model for controls.
/// </summary>
public abstract class ControlViewModelBase : ViewModelBase
{
    /// <summary>
    /// The control associated with this view-model.
    /// </summary>
    public UserControl Control { get; }

    /// <summary>
    /// Creates a view-model for controls.
    /// </summary>
    /// <param name="control">The control associated with this view-model.</param>
    /// <param name="window">The view associated with this view-model.</param>
    public ControlViewModelBase(UserControl control, Window window) : base(window)
        => Control = control;
}