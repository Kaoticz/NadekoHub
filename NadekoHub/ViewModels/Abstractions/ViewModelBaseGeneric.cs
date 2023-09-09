using ReactiveUI;

namespace NadekoHub.ViewModels.Abstractions;

/// <summary>
/// The base view-model.
/// </summary>
/// <typeparam name="T">The type of the view this view-model is associated with.</typeparam>
public abstract class ViewModelBase<T> : ViewModelBase where T : IViewFor
{
    /// <summary>
    /// The type of the view this view-model is associated with.
    /// </summary>
    public Type ViewType { get; } = typeof(T);
}