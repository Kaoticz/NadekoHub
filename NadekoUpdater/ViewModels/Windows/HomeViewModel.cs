using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.ViewModels.Controls;
using NadekoUpdater.Views.Controls;
using NadekoUpdater.Views.Windows;
using System.Diagnostics;

namespace NadekoUpdater.ViewModels.Windows;

/// <summary>
/// View-model for the main window.
/// </summary>
public class HomeViewModel : ViewModelBase<HomeView>
{
    /// <summary>
    /// View-model instance of a <see cref="LateralBarView"/>.
    /// </summary>
    public LateralBarViewModel LateralBarInstance { get; }

    /// <inheritdoc />
    /// <param name="lateralBarInstance">The lateral bar.</param>
    public HomeViewModel(LateralBarViewModel lateralBarInstance)
        => LateralBarInstance = lateralBarInstance;

    /// <summary>
    /// Opens the specified URL in the system's default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public void OpenUrl(string url)
        => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}