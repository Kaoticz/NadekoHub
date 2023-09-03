using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using NadekoUpdater.Enums;
using NadekoUpdater.Extensions;
using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Controls;
using NadekoUpdater.Views.Windows;
using ReactiveUI;
using System.Diagnostics;

namespace NadekoUpdater.ViewModels.Controls;

/// <summary>
/// Defines the view-model for a button that installs a dependency for Nadeko.
/// </summary>
public class DependencyButtonViewModel : ViewModelBase<DependencyButton>
{
    private string _dependencyName = "DependencyName";
    private DependencyStatus _status;
    private bool _isEnabled = false;
    private ImmutableSolidColorBrush _borderColor;
    private readonly AppView _appView;

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
        set => this.RaiseAndSetIfChanged(ref _dependencyName, value);
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
            BorderColor = UpdateButtonColor(value);
            IsEnabled = value is not DependencyStatus.Installed
                and not DependencyStatus.Updating
                and not DependencyStatus.Checking;
        }
    }

    /// <summary>
    /// The current color of the dependency button.
    /// </summary>
    public ImmutableSolidColorBrush BorderColor
    {
        get => _borderColor;
        private set => this.RaiseAndSetIfChanged(ref _borderColor, value);
    }

    /// <summary>
    /// Creates the view-model for a button that installs a dependency for Nadeko.
    /// </summary>
    /// <param name="appView"></param>
    public DependencyButtonViewModel(AppView appView)
    {
        _appView = appView;
        _status = DependencyStatus.Checking;
        _borderColor = UpdateButtonColor(Status);
    }

    public ImmutableSolidColorBrush RecheckButtonColor()
        => BorderColor = UpdateButtonColor(Status);

    /// <summary>
    /// Triggers the <see cref="Click"/> event.
    /// </summary>
    /// <remarks>This method is run everytime the button associated with this view-model is pressed.</remarks>
    protected internal Task RaiseClick()
        => Click?.Invoke(this, EventArgs.Empty) ?? Task.CompletedTask;

    private ImmutableSolidColorBrush UpdateButtonColor(DependencyStatus status)
    {
        return status switch
        {
            DependencyStatus.Install => _appView.GetResource<ImmutableSolidColorBrush>(AppResources.DependencyInstall),
            DependencyStatus.Installed => AppStatics.TransparentColorBrush,
            DependencyStatus.Updating => _appView.GetResource<ImmutableSolidColorBrush>(AppResources.DependencyUpdate),
            DependencyStatus.Update => _appView.GetResource<ImmutableSolidColorBrush>(AppResources.DependencyUpdate),
            DependencyStatus.Checking => AppStatics.TransparentColorBrush,
            _ => throw new UnreachableException($"Dependency status {status} was not implemented."),
        };
    }
}
