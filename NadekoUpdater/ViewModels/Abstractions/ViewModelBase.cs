using ReactiveUI;

namespace NadekoUpdater.ViewModels.Abstractions;

/// <summary>
/// The base view-model.
/// </summary>
public abstract class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    /// <summary>
    /// Activates this view-model with ReactiveUI. See <see cref="ViewModelActivator"/> for more information.
    /// </summary>
    /// <remarks>Activation must also be set up in the corresponding view of this view-model.</remarks>
    public ViewModelActivator Activator { get; } = new();
}