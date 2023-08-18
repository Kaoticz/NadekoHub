using Kotz.Events;
using NadekoUpdater.Enums;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using ReactiveUI;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// Defines the view-model for a button that installs a dependency for Nadeko.
/// </summary>
public class DependencyButtonViewModel : ViewModelBase<DependencyButton>
{
    private string _dependencyName = "DependencyName";
    private DependencyStatus _status = DependencyStatus.Checking;
    private bool _isEnabled = false;

    /// <summary>
    /// Raised when the button associated with this view-model is pressed.
    /// </summary>
    public event AsyncEventHandler<DependencyButtonViewModel, EventArgs>? Click;

    /// <summary>
    /// Determines whether the button associated with this view-model is enabled or not.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        private set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    /// <summary>
    /// The name of the dependency.
    /// </summary>
    public string DependencyName
    {
        get => _dependencyName;
        init => this.RaiseAndSetIfChanged(ref _dependencyName, value);
    }

    /// <summary>
    /// The status of this dependency.
    /// </summary>
    public DependencyStatus Status
    {
        get => _status;
        set
        {
            this.RaiseAndSetIfChanged(ref _status, value);
            IsEnabled = value is not DependencyStatus.Installed
                and not DependencyStatus.Updating
                and not DependencyStatus.Checking;
        }
    }

    /// <summary>
    /// Triggers the <see cref="Click"/> event.
    /// </summary>
    /// <remarks>This method is run everytime the button associated with this view-model is pressed.</remarks>
    protected internal Task RaiseClick()
        => Click?.Invoke(this, EventArgs.Empty) ?? Task.CompletedTask;
}
