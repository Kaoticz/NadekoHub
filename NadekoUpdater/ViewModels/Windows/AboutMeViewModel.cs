using NadekoUpdater.ViewModels.Abstractions;
using NadekoUpdater.Views.Windows;
using System.Diagnostics;

namespace NadekoUpdater.ViewModels.Windows;

/// <summary>
/// View-model for the about me dialog window.
/// </summary>
public class AboutMeViewModel : ViewModelBase<AboutMeView>
{
    /// <summary>
    /// Opens the specified URL in the system's default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public void OpenUrl(string url)
        => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}