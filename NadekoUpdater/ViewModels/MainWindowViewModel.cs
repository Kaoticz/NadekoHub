using System.Diagnostics;
using NadekoUpdater.Views;

namespace NadekoUpdater.ViewModels;

/// <summary>
/// View-model for the main window.
/// </summary>
public sealed class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// View-model instance of a <see cref="LateralBarView"/>.
    /// </summary>
    public LateralBarViewModel LateralBarInstance { get; } = new();

    /// <summary>
    /// Opens the specified URL in the system's default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public void OpenUrl(string url)
        => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}