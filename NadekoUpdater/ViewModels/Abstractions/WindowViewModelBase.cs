using Avalonia.Controls;

namespace NadekoUpdater.ViewModels.Abstractions;

/// <summary>
/// Base view-model for windows.
/// </summary>
public abstract class WindowViewModelBase : ViewModelBase
{
    /// <summary>
    /// The view that owns this view-model.
    /// </summary>
    /// <remarks>Relevant for views that need to lock the parent view when displayed.</remarks>
    public Window? ParentView { get; }

    /// <summary>
    /// Creates a view-model for windows.
    /// </summary>
    /// <param name="window">The view associated with this view-model.</param>
    /// <param name="parentWindow">The view that owns this view-model or <see langword="null"/> if there isn't one.</param>
    public WindowViewModelBase(Window window, Window? parentWindow = null) : base(window)
        => ParentView = parentWindow;
}