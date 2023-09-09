using Avalonia.Controls;
using Avalonia.ReactiveUI;
using NadekoHub.ViewModels.Controls;

namespace NadekoHub.Views.Controls;

/// <summary>
/// The view for the application's settings.
/// </summary>
public partial class ConfigView : ReactiveUserControl<ConfigViewModel>
{
    /// <summary>
    /// Creates a view for the application's settings.
    /// </summary>
    public ConfigView()
        => InitializeComponent();

    /// <summary>
    /// Sets the value of the button spinner when the user spins it.
    /// </summary>
    /// <param name="sender">The <see cref="ButtonSpinner"/>.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void ButtonSpun(object? sender, SpinEventArgs eventArgs)
        => base.ViewModel?.SpinMaxLogSizeAsync(eventArgs.Direction);
}