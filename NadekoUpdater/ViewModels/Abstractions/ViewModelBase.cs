using Avalonia.Controls;
using ReactiveUI;

namespace NadekoUpdater.ViewModels.Abstractions;

/// <summary>
/// Base view-model.
/// </summary>
public abstract class ViewModelBase : ReactiveObject
{
    /// <summary>
    /// The view associated with this view-model.
    /// </summary>
    public Window View { get; }

    /// <summary>
    /// Creates a view-model.
    /// </summary>
    /// <param name="window">The view associated with this view-model.</param>
    public ViewModelBase(Window window)
        => View = window;
}